using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Common.Settings;
using NextErp.Application.Ecommerce;
using NextErp.Application.Handlers.QueryHandlers.Ecommerce;
using NextErp.Application.Interfaces;
using NextErp.Application.Settings;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Ecommerce;

public class CreateOnlineOrderHandler(
    IApplicationDbContext dbContext,
    ISettingsProvider settingsProvider,
    INotificationService notifications)
    : IRequestHandler<CreateOnlineOrderCommand, string>
{
    public async Task<string> Handle(CreateOnlineOrderCommand request, CancellationToken cancellationToken = default)
    {
        var settings = await settingsProvider.GetAsync<EcommerceSettings>();
        if (!settings.StorefrontEnabled)
            throw new InvalidOperationException("The storefront is not available.");

        // Resolve the selling branch the same way the storefront read path does
        // (zero-config: auto-uses the default branch unless branch selling is on),
        // so a blank/garbage SellingBranchId can't break checkout.
        var branchId = await StoreQueryShared.SellingBranchAsync(settingsProvider, dbContext, cancellationToken);

        var variantIds = request.Items.Select(i => i.ProductVariantId).Distinct().ToList();

        // One batched load; loop over the in-memory map (no N+1). Prices are
        // resolved server-side — the client never supplies a price.
        var variants = await dbContext.ProductVariants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(v => variantIds.Contains(v.Id)
                        && v.IsActive
                        && v.Product.IsActive && v.Product.IsPublishedOnline
                        && v.Product.BranchId == branchId
                        && v.Product.Category.IsActive && v.Product.Category.IsPublishedOnline)
            .Select(v => new { v.Id, v.Sku, v.Price, ProductTitle = v.Product.Title, TenantId = v.Product.TenantId })
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        var failures = request.Items
            .Where(i => !variants.ContainsKey(i.ProductVariantId))
            .Select(i => new ValidationFailure("Items", $"Item {i.ProductVariantId} is not available in the store."))
            .ToList();
        if (failures.Count > 0)
            throw new ValidationException(failures);

        var tenantId = variants.Values.First().TenantId;
        var order = new Entities.OnlineOrder
        {
            OrderNumber = await OnlineOrderNumberFactory.NextNumberAsync(tenantId, dbContext, cancellationToken),
            CustomerName = request.CustomerName.Trim(),
            Phone = request.Phone.Trim(),
            Address = request.Address.Trim(),
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            DeliveryFee = settings.DeliveryFee,
            TenantId = tenantId,
            BranchId = branchId,
            CreatedAt = DateTime.UtcNow,
        };
        foreach (var item in request.Items)
        {
            var variant = variants[item.ProductVariantId];
            order.Items.Add(new Entities.OnlineOrderItem
            {
                ProductVariantId = variant.Id,
                ProductTitle = variant.ProductTitle,
                Sku = variant.Sku,
                UnitPrice = variant.Price,
                Quantity = item.Quantity,
                LineTotal = decimal.Round(variant.Price * item.Quantity, 2),
            });
        }

        dbContext.OnlineOrders.Add(order);

        // Read-max-then-increment can race under concurrent checkouts; the
        // (TenantId, OrderNumber) unique index rejects the loser. Regenerate
        // and retry instead of surfacing a 500 to a customer.
        //
        // Retry ONLY the order-number unique violation: a duplicate-key error
        // is statement-scoped on SQL Server (default XACT_ABORT OFF) and on
        // SQLite, so the ambient TransactionBehavior transaction stays usable.
        // Anything else (e.g. a deadlock, which dooms the transaction and
        // must not be retried on the same connection) falls straight through
        // to the DbUpdateException -> 409 mapping in ApiExceptionHandler.
        const int maxAttempts = 3;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                break;
            }
            catch (DbUpdateException ex) when (
                attempt < maxAttempts
                && ex.InnerException?.Message?.Contains("OrderNumber") == true)
            {
                order.OrderNumber = await OnlineOrderNumberFactory.NextNumberAsync(tenantId, dbContext, cancellationToken);
            }
        }

        // Staged after the save succeeds so the message always carries the
        // final, persisted order number (never a number that lost the race).
        await notifications.RecordAsync(
            type: "OnlineOrderPlaced",
            title: "New online order",
            message: $"{order.OrderNumber} — {order.CustomerName}",
            relatedEntityType: "OnlineOrder",
            relatedEntityId: order.OrderNumber,
            cancellationToken: cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return order.OrderNumber;
    }
}
