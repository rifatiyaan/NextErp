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
            // Get all variation options with their values
            var allOptions = await dbContext.VariationOptions
                .Include(vo => vo.Values)
                .Where(vo => vo.IsActive)
                .ToListAsync(cancellationToken);

            // Group by option name and aggregate distinct values
            var groupedOptions = allOptions
                .GroupBy(vo => vo.Name)
                .Select(g => new ProductVariation.Response.BulkVariationOptionDto
                {
                    Name = g.Key,
                    Values = g
                        .SelectMany(vo => vo.Values)
                        .Where(v => v.IsActive)
                        .Select(v => v.Value)
                        .Distinct()
                        .OrderBy(v => v)
                        .ToList()
                })
                .OrderBy(dto => dto.Name)
                .ToList();

            return groupedOptions;
        }
    }
}

