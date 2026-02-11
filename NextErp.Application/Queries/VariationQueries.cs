using MediatR;
using NextErp.Application.DTOs;
using NextErp.Domain.Entities;

namespace NextErp.Application.Queries
{
    public record GetVariationOptionsByProductIdQuery(int ProductId) : IRequest<List<VariationOption>>;
    
    public record GetVariationOptionByIdQuery(int Id) : IRequest<VariationOption?>;
    
    public record GetVariationValueByIdQuery(int Id) : IRequest<VariationValue?>;

    public record GetAllDistinctVariationOptionsQuery() : IRequest<List<DTOs.ProductVariation.Response.BulkVariationOptionDto>>;
}

