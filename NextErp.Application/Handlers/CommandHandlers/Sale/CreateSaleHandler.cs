using NextErp.Application.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Sale
{
    public class CreateSaleHandler(
        IApplicationUnitOfWork unitOfWork,
        IApplicationDbContext dbContext,
        IStockService stockService,
        IBranchProvider branchProvider)
        : IRequestHandler<CreateSaleCommand, Guid>
    {
        public async Task<Guid> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            if (request.Items.Count == 0)
                throw new InvalidOperationException("A sale must contain at least one line item.");

            var variantIds = request.Items.Select(i => i.ProductVariantId).Distinct().ToList();
            var variants = await dbContext.ProductVariants
                .Include(v => v.Product)
                .Where(v => variantIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id, cancellationToken);

            if (variants.Count != variantIds.Count)
            {
                var missing = variantIds.Except(variants.Keys).ToList();
                throw new InvalidOperationException(
                    $"Product variant(s) not found: {string.Join(", ", missing)}.");
            }

            var tenantId = variants.Values.First().TenantId;
            var branchId = ResolveWriteBranchId(variants.Values);

            var normalizedItems = new List<(Entities.ProductVariant Variant, decimal Quantity, decimal UnitPrice)>(request.Items.Count);
            foreach (var itemDto in request.Items)
            {
                if (itemDto.Quantity <= 0)
                    throw new InvalidOperationException("Sale item quantity must be greater than zero.");

                var variant = variants[itemDto.ProductVariantId];
                var unitPrice = variant.Price;
                normalizedItems.Add((variant, itemDto.Quantity, unitPrice));
            }

            var grossTotal = normalizedItems.Sum(l => l.UnitPrice * l.Quantity);

            foreach (var line in normalizedItems)
            {
                var variant = line.Variant;
                await stockService.EnsureStockRecordExistsAsync(variant.Id, cancellationToken);

                var isAvailable = await stockService.CheckStockAvailabilityAsync(
                    variant.Id,
                    line.Quantity,
                    cancellationToken);

                if (isAvailable)
                    continue;

                var available = await stockService.GetAvailableStockAsync(variant.Id, cancellationToken);
                throw new InvalidOperationException(
                    $"Insufficient stock for SKU '{variant.Sku}' ({variant.Product?.Title}). " +
                    $"Available: {available}, Required: {line.Quantity}.");
            }

            var saleNumber = $"SALE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            var sale = Entities.Sale.Create(
                id: Guid.NewGuid(),
                saleNumber: saleNumber,
                title: $"Sale - {DateTime.UtcNow:yyyy-MM-dd HH:mm}",
                tenantId: tenantId,
                branchId: branchId,
                partyId: request.PartyId,
                itemsGrossTotal: grossTotal,
                discountRequested: request.Discount,
                taxRate: Entities.Sale.DefaultTaxRate,
                saleDate: DateTime.UtcNow,
                metadata: new Entities.Sale.SaleMetadata
                {
                    PaymentMethod = request.PaymentMethod
                });

            await unitOfWork.SaleRepository.AddAsync(sale);

            foreach (var line in normalizedItems)
            {
                var variant = line.Variant;
                var lineTitle = $"{variant.Product?.Title ?? "Product"} — {variant.Title}";

                var item = new Entities.SaleItem
                {
                    Id = Guid.NewGuid(),
                    Title = lineTitle,
                    SaleId = sale.Id,
                    ProductVariantId = variant.Id,
                    Quantity = line.Quantity,
                    Price = line.UnitPrice,
                    CreatedAt = DateTime.UtcNow,
                    TenantId = sale.TenantId
                };

                sale.Items.Add(item);

                await stockService.RecordMovementAsync(
                    variant.Id,
                    sale.TenantId,
                    sale.BranchId,
                    -line.Quantity,
                    Entities.StockMovementType.Sale,
                    sale.Id,
                    cancellationToken);
            }

            if (ShouldCreatePayment(request, sale.FinalAmount))
            {
                var paymentMethod = ResolvePaymentMethod(request.PaymentMethod);
                var amount = Math.Min(request.PaidAmount ?? 0m, sale.FinalAmount);
                var payment = new Entities.SalePayment
                {
                    Id = Guid.NewGuid(),
                    Title = $"Payment — {DateTime.UtcNow:yyyy-MM-dd}",
                    SaleId = sale.Id,
                    Amount = amount,
                    PaymentMethod = paymentMethod,
                    PaidAt = DateTime.UtcNow,
                    TenantId = sale.TenantId,
                    CreatedAt = DateTime.UtcNow
                };
                await dbContext.SalePayments.AddAsync(payment, cancellationToken);
            }

            try
            {
                await unitOfWork.SaveAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("Stock was modified by another transaction. Please retry.");
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw new InvalidOperationException($"Failed to create sale: {ex.Message}", ex);
            }

            return sale.Id;
        }

        private static bool ShouldCreatePayment(CreateSaleCommand request, decimal finalAmount)
        {
            return !string.IsNullOrWhiteSpace(request.PaymentMethod)
                   && request.PaidAmount.HasValue
                   && request.PaidAmount.Value > 0
                   && finalAmount > 0;
        }

        private static Entities.PaymentMethodType ResolvePaymentMethod(string? paymentMethod)
        {
            if (Enum.TryParse<Entities.PaymentMethodType>(paymentMethod, true, out var parsed))
                return parsed;

            return Entities.PaymentMethodType.Other;
        }

        private Guid ResolveWriteBranchId(IEnumerable<Entities.ProductVariant> variants)
        {
            if (!branchProvider.IsGlobal())
                return branchProvider.GetRequiredBranchId();

            var claimBranchId = branchProvider.GetBranchId();
            if (claimBranchId.HasValue)
                return claimBranchId.Value;

            var variantBranchId = variants.Select(v => v.BranchId).FirstOrDefault(b => b.HasValue);
            if (variantBranchId.HasValue)
                return variantBranchId.Value;

            throw new InvalidOperationException("Global user must provide a branch context to create a sale.");
        }
    }
}
