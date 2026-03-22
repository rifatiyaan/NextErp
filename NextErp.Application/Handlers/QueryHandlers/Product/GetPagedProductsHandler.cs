using NextErp.Application;
using NextErp.Application.Common;
using NextErp.Application.Queries;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = NextErp.Domain.Entities;
using ProductDto = NextErp.Application.DTOs.Product;

namespace NextErp.Application.Handlers.QueryHandlers.Product
{
    public class GetPagedProductsHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetPagedProductsQuery, PagedResult<ProductDto.Response.Get.Single>>
    {
        public async Task<PagedResult<ProductDto.Response.Get.Single>> Handle(
            GetPagedProductsQuery request,
            CancellationToken cancellationToken)
        {
            var query = unitOfWork.ProductRepository.Query();

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = request.Status.ToLowerInvariant() switch
                {
                    "active" => query.Where(p => p.IsActive),
                    "out of stock" => query.Where(p =>
                        p.IsActive && !p.ProductVariants.Any(v => v.Stock > 0)),
                    "closed" => query.Where(p => !p.IsActive),
                    _ => query.Where(p => p.IsActive),
                };
            }
            else
            {
                query = query.Where(p => p.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var searchText = request.SearchText.Trim();
                var searchPattern = $"%{searchText}%";
                query = query.Where(p =>
                    EF.Functions.Like(p.Title, searchPattern) ||
                    EF.Functions.Like(p.Code, searchPattern));
            }

            if (request.CategoryId is > 0)
            {
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);
            }

            var total = await query.CountAsync(cancellationToken);

            query = request.SortBy?.ToLowerInvariant() switch
            {
                "title" => query.OrderBy(p => p.Title),
                "price" => query.OrderBy(p => p.Price),
                _ => query.OrderByDescending(p => p.CreatedAt),
            };

            var records = await query
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = mapper.Map<List<ProductDto.Response.Get.Single>>(records);

            if (request.IncludeStock && dtos.Count > 0)
            {
                var ids = records.Select(p => p.Id).Distinct().ToArray();
                var aggregates =
                    await unitOfWork.StockRepository.GetProductStockAggregatesAsync(ids,
                        cancellationToken);
                var lookup = aggregates.ToDictionary(
                    t => t.ProductId,
                    t => (t.TotalAvailable, t.HasLowStock));

                ApplyStockColumns(dtos, lookup);
            }

            return new PagedResult<ProductDto.Response.Get.Single>(dtos, total, total);
        }

        private static void ApplyStockColumns(
            List<ProductDto.Response.Get.Single> dtos,
            IReadOnlyDictionary<int, (decimal TotalAvailable, bool HasLowStock)> lookup)
        {
            foreach (var d in dtos)
            {
                if (lookup.TryGetValue(d.Id, out var row))
                {
                    d.TotalAvailableQuantity = row.TotalAvailable;
                    d.HasLowStock = row.HasLowStock;
                }
                else
                {
                    d.TotalAvailableQuantity = 0m;
                    d.HasLowStock = false;
                }
            }
        }
    }
}
