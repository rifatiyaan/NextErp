using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Sale;

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

        var variants = await LoadVariantsByRequestAsync(request, cancellationToken);
        var tenantId = variants.Values.First().TenantId;
        var branchId = ResolveWriteBranchId(variants.Values);

        var lines = NormalizeSaleLines(request, variants);
        var grossTotal = lines.Sum(l => l.UnitPrice * l.Quantity);

        await EnsureStockAvailableForAllLinesAsync(lines, cancellationToken);

        var sale = CreateSaleEntity(request, tenantId, branchId, grossTotal);
        await unitOfWork.SaleRepository.AddAsync(sale);

        await AddSaleLinesAndStockMovementsAsync(sale, lines, cancellationToken);
        await AddOptionalPaymentAsync(request, sale, cancellationToken);

        await unitOfWork.SaveAsync();
        return sale.Id;
    }

    private async Task<Dictionary<int, Entities.ProductVariant>> LoadVariantsByRequestAsync(
        CreateSaleCommand request,
        CancellationToken cancellationToken)
    {
        var variantIds = request.Items.Select(i => i.ProductVariantId).Distinct().ToList();
        var variants = await dbContext.ProductVariants
            .Include(v => v.Product)
            .Where(v => variantIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        if (variants.Count == variantIds.Count)
            return variants;

        var missing = string.Join(", ", variantIds.Except(variants.Keys));
        throw new InvalidOperationException($"Product variant(s) not found: {missing}.");
    }

    private static List<(Entities.ProductVariant Variant, decimal Quantity, decimal UnitPrice)> NormalizeSaleLines(
        CreateSaleCommand request,
        IReadOnlyDictionary<int, Entities.ProductVariant> variants)
    {
        return request.Items.Select(dto =>
        {
            if (dto.Quantity <= 0)
                throw new InvalidOperationException("Sale item quantity must be greater than zero.");

            var variant = variants[dto.ProductVariantId];
            return (variant, dto.Quantity, variant.Price);
        }).ToList();
    }

    private async Task EnsureStockAvailableForAllLinesAsync(
        IReadOnlyList<(Entities.ProductVariant Variant, decimal Quantity, decimal UnitPrice)> lines,
        CancellationToken cancellationToken)
    {
        foreach (var (variant, quantity, _) in lines)
        {
            await stockService.EnsureStockRecordExistsAsync(variant.Id, cancellationToken);

            if (await stockService.CheckStockAvailabilityAsync(variant.Id, quantity, cancellationToken))
                continue;

            var available = await stockService.GetAvailableStockAsync(variant.Id, cancellationToken);
            var productTitle = variant.Product?.Title ?? "Product";
            throw new InvalidOperationException(
                $"Insufficient stock for SKU '{variant.Sku}' ({productTitle}). " +
                $"Available: {available}, Required: {quantity}.");
        }
    }

    private static Entities.Sale CreateSaleEntity(
        CreateSaleCommand request,
        Guid tenantId,
        Guid branchId,
        decimal grossTotal)
    {
        var suffix = Guid.NewGuid().ToString()[..8].ToUpperInvariant();
        var saleNumber = $"SALE-{DateTime.UtcNow:yyyyMMdd}-{suffix}";

        return Entities.Sale.Create(
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
            metadata: new Entities.Sale.SaleMetadata { PaymentMethod = request.PaymentMethod });
    }

    private async Task AddSaleLinesAndStockMovementsAsync(
        Entities.Sale sale,
        IReadOnlyList<(Entities.ProductVariant Variant, decimal Quantity, decimal UnitPrice)> lines,
        CancellationToken cancellationToken)
    {
        foreach (var (variant, quantity, unitPrice) in lines)
        {
            var lineTitle = $"{variant.Product?.Title ?? "Product"} — {variant.Title}";
            sale.Items.Add(new Entities.SaleItem
            {
                Id = Guid.NewGuid(),
                Title = lineTitle,
                SaleId = sale.Id,
                ProductVariantId = variant.Id,
                Quantity = quantity,
                Price = unitPrice,
                CreatedAt = DateTime.UtcNow,
                TenantId = sale.TenantId
            });

            await stockService.RecordMovementAsync(
                variant.Id,
                sale.TenantId,
                sale.BranchId,
                -quantity,
                Entities.StockMovementType.Sale,
                sale.Id,
                cancellationToken);
        }
    }

    private async Task AddOptionalPaymentAsync(
        CreateSaleCommand request,
        Entities.Sale sale,
        CancellationToken cancellationToken)
    {
        if (!ShouldCreatePayment(request, sale.FinalAmount))
            return;

        var method = Enum.TryParse<Entities.PaymentMethodType>(request.PaymentMethod, true, out var parsed)
            ? parsed
            : Entities.PaymentMethodType.Other;

        var amount = Math.Min(request.PaidAmount ?? 0m, sale.FinalAmount);
        await dbContext.SalePayments.AddAsync(new Entities.SalePayment
        {
            Id = Guid.NewGuid(),
            Title = $"Payment — {DateTime.UtcNow:yyyy-MM-dd}",
            SaleId = sale.Id,
            Amount = amount,
            PaymentMethod = method,
            PaidAt = DateTime.UtcNow,
            TenantId = sale.TenantId,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
    }

    private static bool ShouldCreatePayment(CreateSaleCommand request, decimal finalAmount) =>
        !string.IsNullOrWhiteSpace(request.PaymentMethod)
        && request.PaidAmount is > 0
        && finalAmount > 0;

    private Guid ResolveWriteBranchId(IEnumerable<Entities.ProductVariant> variants)
    {
        if (!branchProvider.IsGlobal())
            return branchProvider.GetRequiredBranchId();

        var claimBranchId = branchProvider.GetBranchId();
        if (claimBranchId.HasValue)
            return claimBranchId.Value;

        var fromVariant = variants.Select(v => v.BranchId).FirstOrDefault(b => b.HasValue);
        if (fromVariant.HasValue)
            return fromVariant.Value;

        throw new InvalidOperationException("Global user must provide a branch context to create a sale.");
    }
}
