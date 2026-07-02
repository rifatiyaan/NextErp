using Riok.Mapperly.Abstractions;
using NextErp.Application.DTOs.Payment;
using NextErp.Application.DTOs.Sale;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Mapping;

// SaleResponse + SaleItemResponse are hand-written below — Mapperly can't
// express the Payments ordering, TotalPaid/BalanceDue sums, or null-safe nav
// fallbacks. The rest are source-generated.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
internal static partial class SaleMappers
{
    // Straight field copy (names match): Id, Title, SaleNumber, TotalAmount, CreatedAt.
    internal static partial CreateSaleResponse ToCreateResponse(this Entities.Sale s);

    // Straight field copy (names match): ReferenceNo, PaymentMethod, Notes.
    private static partial SaleMetadataRequest ToMetadataRequest(this Entities.Sale.SaleMetadata m);

    /// <summary>
    /// Entity -> <see cref="SaleResponse"/>. Mirrors SaleProfile:
    /// CustomerName falls back to "Unknown"; Payments are ordered by PaidAt
    /// then CreatedAt; TotalPaid is the payment sum; BalanceDue is
    /// FinalAmount − TotalPaid.
    /// </summary>
    internal static SaleResponse ToResponse(this Entities.Sale s)
    {
        var totalPaid = s.Payments.Sum(p => p.Amount);
        return new SaleResponse
        {
            Id = s.Id,
            Title = s.Title,
            SaleNumber = s.SaleNumber,
            PartyId = s.PartyId,
            CustomerName = s.Party != null ? s.Party.Title : "Unknown",
            SaleDate = s.SaleDate,
            TotalAmount = s.TotalAmount,
            Discount = s.Discount,
            Tax = s.Tax,
            FinalAmount = s.FinalAmount,
            TotalPaid = totalPaid,
            BalanceDue = s.FinalAmount - totalPaid,
            Items = s.Items.Select(i => i.ToResponse()).ToList(),
            Payments = s.Payments
                .OrderBy(p => p.PaidAt)
                .ThenBy(p => p.CreatedAt)
                .Select(p => p.ToResponse())
                .ToList(),
            Metadata = s.Metadata.ToMetadataRequest(),
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            TenantId = s.TenantId,
            BranchId = s.BranchId,
        };
    }

    /// <summary>
    /// Entity -> <see cref="SaleItemResponse"/>. Mirrors SaleProfile:
    /// null-safe ProductTitle ("Unknown") / VariantSku ("") / VariantTitle
    /// ("") navigation, and the nullable DiscountSource enum rendered as its
    /// name (Manual/Promotion) or null.
    /// </summary>
    internal static SaleItemResponse ToResponse(this Entities.SaleItem i) => new()
    {
        Id = i.Id,
        Title = i.Title,
        ProductVariantId = i.ProductVariantId,
        ProductTitle = i.ProductVariant != null && i.ProductVariant.Product != null
            ? i.ProductVariant.Product.Title
            : "Unknown",
        VariantSku = i.ProductVariant != null ? i.ProductVariant.Sku : "",
        VariantTitle = i.ProductVariant != null ? i.ProductVariant.Title : "",
        Quantity = i.Quantity,
        UnitPrice = i.UnitPrice,
        Discount = i.Discount,
        DiscountSource = i.DiscountSource.HasValue ? i.DiscountSource.Value.ToString() : null,
        PromotionId = i.PromotionId,
        Total = i.Total,
    };
}
