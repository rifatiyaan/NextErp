using Riok.Mapperly.Abstractions;
using NextErp.Application.DTOs.Promotion;
using DomainPromotion = NextErp.Domain.Entities.Promotion;

namespace NextErp.Application.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
internal static partial class PromotionMappers
{
    internal static partial DomainPromotion ToEntity(this CreatePromotionRequest request);

    internal static partial void ApplyTo(this UpdatePromotionRequest request, DomainPromotion entity);

    internal static partial PromotionResponse ToResponse(this DomainPromotion entity);
}
