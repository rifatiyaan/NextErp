using Riok.Mapperly.Abstractions;
using NextErp.Application.DTOs.Stock;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
internal static partial class StockMappers
{
    // Entity -> StockResponse is hand-written (not a Mapperly partial): the
    // nav-flatten projections carry literal "Unknown"/"N/A"/"" fallbacks for
    // null nav chains, which Mapperly can't emit automatically. The expressions
    // below reproduce the former StockProfile exactly.
    internal static StockResponse ToResponse(this Entities.Stock src) => new()
    {
        Id = src.Id,
        ProductVariantId = src.ProductVariantId,
        ProductId = src.ProductVariant != null ? src.ProductVariant.ProductId : 0,
        ProductTitle = src.ProductVariant != null && src.ProductVariant.Product != null
            ? src.ProductVariant.Product.Title
            : "Unknown",
        ProductCode = src.ProductVariant != null && src.ProductVariant.Product != null
            ? src.ProductVariant.Product.Code
            : "N/A",
        VariantSku = src.ProductVariant != null ? src.ProductVariant.Sku : "",
        VariantTitle = src.ProductVariant != null ? src.ProductVariant.Title : "",
        AvailableQuantity = src.AvailableQuantity,
        ReorderLevel = src.ReorderLevel,
        UnitOfMeasureId = src.ProductVariant != null && src.ProductVariant.Product != null
            ? src.ProductVariant.Product.UnitOfMeasureId
            : null,
        UnitOfMeasureAbbreviation = src.ProductVariant != null && src.ProductVariant.Product != null && src.ProductVariant.Product.UnitOfMeasure != null
            ? src.ProductVariant.Product.UnitOfMeasure.Abbreviation
            : null,
        CreatedAt = src.CreatedAt,
        UpdatedAt = src.UpdatedAt,
        TenantId = src.TenantId,
        BranchId = src.BranchId,
    };
}
