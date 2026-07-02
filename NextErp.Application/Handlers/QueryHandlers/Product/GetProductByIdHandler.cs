using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs.Product;
using NextErp.Application.Interfaces;
using NextErp.Application.Mapping;
using NextErp.Application.Products;
using NextErp.Application.Queries;

namespace NextErp.Application.Handlers.QueryHandlers.Product;

public class GetProductByIdHandler(
    IApplicationDbContext dbContext,
    IBranchProvider branchProvider)
    : IRequestHandler<GetProductByIdQuery, ProductResponse?>
{
    public async Task<ProductResponse?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products
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

        var dto = product.ToResponse();
        await ProductVariantStockLookup.EnrichProductVariantStocksAsync(dto, dbContext, branchProvider, cancellationToken)
            .ConfigureAwait(false);

        dto.TotalAvailableQuantity = await ProductVariantStockLookup.GetProductAggregateStockTotalAsync(
                product.Id,
                dbContext,
                cancellationToken)
            .ConfigureAwait(false);

        return dto;
    }
}
