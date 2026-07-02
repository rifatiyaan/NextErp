using Riok.Mapperly.Abstractions;
using NextErp.Application.DTOs.ProductVariation;
using VariationOptionEntity = NextErp.Domain.Entities.VariationOption;
using VariationValueEntity = NextErp.Domain.Entities.VariationValue;
using ProductVariantEntity = NextErp.Domain.Entities.ProductVariant;

namespace NextErp.Application.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
internal static partial class VariationMappers
{
    // ===== Entity -> Response =====

    // VariationOption.Values are ordered by DisplayOrder before projection
    // (ProductProfile: ForMember(Values, MapFrom(src.Values.OrderBy(DisplayOrder)))).
    [MapProperty(nameof(VariationOptionEntity.Values), nameof(VariationOptionResponse.Values), Use = nameof(OrderedValues))]
    internal static partial VariationOptionResponse ToResponse(this VariationOptionEntity entity);

    internal static partial VariationValueResponse ToResponse(this VariationValueEntity entity);

    // AvailableQuantity has no entity counterpart (it lives on Stock) and is populated
    // later by ProductVariantStockLookup; RequiredMappingStrategy.None leaves it at 0.
    internal static partial ProductVariantResponse ToResponse(this ProductVariantEntity entity);

    private static List<VariationValueResponse> OrderedValues(ICollection<VariationValueEntity> values) =>
        values.OrderBy(v => v.DisplayOrder).Select(v => v.ToResponse()).ToList();

    // ===== Request -> Entity (variant create/update inside the WithVariations handlers) =====
    // Sku/Price/IsActive copy straight. Title/Name/ProductId/VariationValues/audit fields are
    // assigned by the handler after mapping, so they are intentionally left unmapped here.
    // InitialStock and VariationValueKeys have no entity counterpart (handled separately by
    // the stock service / ConfigurableProductVariantFactory).

    [MapperIgnoreSource(nameof(ProductVariantRequest.InitialStock))]
    [MapperIgnoreSource(nameof(ProductVariantRequest.VariationValueKeys))]
    internal static partial ProductVariantEntity ToEntity(this ProductVariantRequest request);

    [MapperIgnoreSource(nameof(ProductVariantRequest.InitialStock))]
    [MapperIgnoreSource(nameof(ProductVariantRequest.VariationValueKeys))]
    internal static partial void ApplyTo(this ProductVariantRequest request, ProductVariantEntity entity);
}
