using AutoMapper;
using NextErp.Application.Commands;
using MediatR;
using NextErp.Application.Interfaces;
using NextErp.Application.Products;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Product
{
    public class CreateProductHandler(
        IApplicationDbContext dbContext,
        IStockService stockService,
        IBranchProvider branchProvider,
        IMapper mapper)
        : IRequestHandler<CreateProductCommand, int>
    {
        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken = default)
        {
            var product = mapper.Map<Entities.Product>(request);
            await ProductBranchScope.ApplyToProductAsync(product, dbContext, branchProvider, cancellationToken);
            product.IsActive = true;
            product.HasVariations = false;
            product.CreatedAt = DateTime.UtcNow;

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync(cancellationToken);

            await ProductGallerySync.ApplyFullGalleryAsync(
                product,
                request.ImageGallery,
                dbContext,
                cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            var variant = Entities.SimpleProductVariantFactory.CreateDefault(product);
            await dbContext.ProductVariants.AddAsync(variant, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            await stockService.EnsureStockRecordExistsAsync(variant.Id, cancellationToken);
            await stockService.SetAvailableQuantityAsync(variant.Id, request.InitialStock, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            return product.Id;
        }
    }
}
