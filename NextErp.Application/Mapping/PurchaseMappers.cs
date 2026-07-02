using Riok.Mapperly.Abstractions;
using NextErp.Application.DTOs.Purchase;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
internal static partial class PurchaseMappers
{
    // Purchase -> PurchaseResponse.
    // Hand-written because SupplierName falls back to "Unknown" when the
    // supplier (Party) navigation is null, and Items maps element-by-element
    // through the hand-written PurchaseItem mapper below. Everything else is a
    // straight field copy (BranchId widens Guid -> Guid?).
    internal static PurchaseResponse ToResponse(this Entities.Purchase entity)
    {
        return new PurchaseResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            PurchaseNumber = entity.PurchaseNumber,
            PartyId = entity.PartyId,
            SupplierName = entity.Party != null ? entity.Party.Title : "Unknown",
            PurchaseDate = entity.PurchaseDate,
            TotalAmount = entity.TotalAmount,
            Discount = entity.Discount,
            NetTotal = entity.NetTotal,
            Items = entity.Items.Select(i => i.ToResponse()).ToList(),
            Metadata = entity.Metadata.ToRequest(),
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            TenantId = entity.TenantId,
            BranchId = entity.BranchId,
        };
    }

    // PurchaseItem -> PurchaseItemResponse.
    // Hand-written: ProductTitle/VariantSku/VariantTitle read through the
    // ProductVariant (and Product) navigations with null-safe fallbacks, and
    // DiscountSource is projected enum -> string for the payload.
    internal static PurchaseItemResponse ToResponse(this Entities.PurchaseItem entity)
    {
        return new PurchaseItemResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            ProductVariantId = entity.ProductVariantId,
            ProductTitle = entity.ProductVariant != null && entity.ProductVariant.Product != null
                ? entity.ProductVariant.Product.Title
                : "Unknown",
            VariantSku = entity.ProductVariant != null ? entity.ProductVariant.Sku : "",
            VariantTitle = entity.ProductVariant != null ? entity.ProductVariant.Title : "",
            Quantity = entity.Quantity,
            UnitCost = entity.UnitCost,
            Discount = entity.Discount,
            DiscountSource = entity.DiscountSource.HasValue ? entity.DiscountSource.Value.ToString() : null,
            Total = entity.Total,
            Metadata = entity.Metadata.ToResponse(),
        };
    }

    // Purchase -> CreatePurchaseResponse (straight name map).
    internal static partial CreatePurchaseResponse ToCreateResponse(this Entities.Purchase entity);

    // Metadata maps (both directions, straight name maps).
    internal static partial PurchaseMetadataRequest ToRequest(this Entities.Purchase.PurchaseMetadata entity);

    internal static partial Entities.Purchase.PurchaseMetadata ToEntity(this PurchaseMetadataRequest request);

    internal static partial PurchaseItemMetadataResponse ToResponse(this Entities.PurchaseItem.PurchaseItemMetadata entity);

    internal static partial Entities.PurchaseItem.PurchaseItemMetadata ToEntity(this PurchaseItemMetadataResponse response);

    internal static partial PurchaseItemMetadataRequest ToItemRequest(this Entities.PurchaseItem.PurchaseItemMetadata entity);

    internal static partial Entities.PurchaseItem.PurchaseItemMetadata ToEntity(this PurchaseItemMetadataRequest request);
}
