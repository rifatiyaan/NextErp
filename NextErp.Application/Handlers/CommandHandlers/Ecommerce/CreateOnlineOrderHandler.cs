using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Common.Settings;
using NextErp.Application.Ecommerce;
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
        if (!settings.StorefrontEnabled || !Guid.TryParse(settings.SellingBranchId, out var branchId))
            throw new InvalidOperationException("The storefront is not available.");

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
