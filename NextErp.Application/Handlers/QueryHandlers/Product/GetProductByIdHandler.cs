using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Products;
using NextErp.Application.Queries;
using ProductGetDto = NextErp.Application.DTOs.Product.Response.Get;
using Repositories = NextErp.Domain.Repositories;

namespace NextErp.Application.Handlers.QueryHandlers.Product;

public class GetProductByIdHandler(
    Repositories.IProductRepository productRepo,
    IApplicationDbContext dbContext,
    IBranchProvider branchProvider,
    IMapper mapper)
    : IRequestHandler<GetProductByIdQuery, ProductGetDto.Single?>
{
    public async Task<ProductGetDto.Single?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await productRepo.Query()
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.UnitOfMeasure)
            .Include(p => p.ProductImages)
            .Include(p => p.Parent)
            .Include(p => p.Children)
            .Include(p => p.ProductVariationOptions)
                .ThenInclude(pvo => pvo.VariationOption)
                .ThenInclude(vo => vo.Values)
            .Include(p => p.ProductVariants)
                .ThenInclude(pv => pv.VariationValues)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (product == null)
            return null;

        var dto = mapper.Map<ProductGetDto.Single>(product);
        await ProductVariantStockLookup.EnrichProductVariantStocksAsync(dto, dbContext, branchProvider, cancellationToken)
            .ConfigureAwait(false);

        dto.Stock = await ProductVariantStockLookup.GetProductAggregateStockTotalAsync(
                product.Id,
                dbContext,
                cancellationToken)
            .ConfigureAwait(false);

        return dto;
    }
}
