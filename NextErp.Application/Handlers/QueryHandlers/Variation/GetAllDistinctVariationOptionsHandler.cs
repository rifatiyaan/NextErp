using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using NextErp.Application.DTOs.ProductVariation;

namespace NextErp.Application.Handlers.QueryHandlers.Variation
{
    public class GetAllDistinctVariationOptionsHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetAllDistinctVariationOptionsQuery, List<BulkVariationOptionResponse>>
    {
        public async Task<List<BulkVariationOptionResponse>> Handle(
            GetAllDistinctVariationOptionsQuery request,
            CancellationToken cancellationToken = default)
        {
            return await dbContext.VariationOptions
                .AsNoTracking()
                .Include(vo => vo.Values)
                .Where(vo => vo.IsActive)
                .OrderBy(vo => vo.DisplayOrder)
                .ThenBy(vo => vo.Name)
                .Select(vo => new BulkVariationOptionResponse
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

