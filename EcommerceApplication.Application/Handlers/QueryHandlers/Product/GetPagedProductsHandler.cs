using EcommerceApplicationWeb.Application.Common;
using EcommerceApplicationWeb.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = EcommerceApplicationWeb.Domain.Entities;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Handlers.QueryHandlers.Product
{
    public class GetPagedProductsHandler : IRequestHandler<GetPagedProductsQuery, PagedResult<Entities.Product>>
    {
        private readonly Repositories.IProductRepository _productRepo;

        public GetPagedProductsHandler(Repositories.IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<PagedResult<Entities.Product>> Handle(
            GetPagedProductsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _productRepo.Query().Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(p => p.Title.Contains(request.SearchText));

            var total = await query.CountAsync(cancellationToken);

            query = request.SortBy?.ToLower() switch
            {
                "title" => query.OrderBy(p => p.Title),
                "price" => query.OrderBy(p => p.Price),
                _ => query.OrderBy(p => p.Id)
            };

            var records = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Entities.Product>(records, total, total);
        }
    }
}
