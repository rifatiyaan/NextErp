using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using NextErp.Application.DTOs;

namespace NextErp.Application.Handlers.QueryHandlers.Variation
{
    public class GetAllDistinctVariationOptionsHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetAllDistinctVariationOptionsQuery, List<ProductVariation.Response.BulkVariationOptionDto>>
    {
        public async Task<List<ProductVariation.Response.BulkVariationOptionDto>> Handle(
            GetAllDistinctVariationOptionsQuery request,
            CancellationToken cancellationToken)
        {
            return await dbContext.VariationOptions
                .AsNoTracking()
                .Include(vo => vo.Values)
                .Where(vo => vo.IsActive)
                .OrderBy(vo => vo.DisplayOrder)
                .ThenBy(vo => vo.Name)
                .Select(vo => new ProductVariation.Response.BulkVariationOptionDto
                {
                    Name = vo.Name,
                    Values = vo.Values
                        .Where(v => v.IsActive)
                        .OrderBy(v => v.DisplayOrder)
                        .Select(v => v.Value)
                        .ToList()
                })
                .ToListAsync(cancellationToken);
        }
    }
}

