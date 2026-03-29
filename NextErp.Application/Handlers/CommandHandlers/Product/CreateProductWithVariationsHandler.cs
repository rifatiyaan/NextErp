using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using NextErp.Application.Products;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Product
{
    public class CreateProductWithVariationsHandler(
        IApplicationUnitOfWork unitOfWork,
        IApplicationDbContext dbContext,
        IStockService stockService,
        IMapper mapper)
        : IRequestHandler<CreateProductWithVariationsCommand, int>
    {
        public async Task<int> Handle(CreateProductWithVariationsCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted,
                cancellationToken);

            try
            {
                var product = mapper.Map<Entities.Product>(request);
                product.HasVariations = true;
                product.CreatedAt = DateTime.UtcNow;

                await unitOfWork.ProductRepository.AddAsync(product);
                await unitOfWork.SaveAsync();

                var optionByName = await ConfigurableProductVariantFactory.LoadActiveGlobalOptionsAsync(
                    dbContext,
                    request.VariationOptions.Select(o => o.Name),
                    cancellationToken);

                await ConfigurableProductVariantFactory.SyncVariationValuesFromRequestAsync(
                    request.VariationOptions,
                    optionByName,
                    dbContext,
                    cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                optionByName = await ConfigurableProductVariantFactory.LoadActiveGlobalOptionsAsync(
                    dbContext,
                    request.VariationOptions.Select(o => o.Name),
                    cancellationToken);

                await AddProductVariationOptionsAsync(product.Id, request, optionByName, dbContext, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                var valueKeyMap = ConfigurableProductVariantFactory.BuildValueKeyMap(
                    request.VariationOptions,
                    optionByName);

                foreach (var variantDto in request.ProductVariants)
                {
                    var values = ConfigurableProductVariantFactory.ResolveVariationValues(
                        variantDto.VariationValueKeys,
                        valueKeyMap);

                    var title = ConfigurableProductVariantFactory.BuildDisplayTitle(values);
                    var productVariant = mapper.Map<Entities.ProductVariant>(variantDto);
                    productVariant.Title = title;
                    productVariant.Name = title;
                    productVariant.ProductId = product.Id;
                    productVariant.CreatedAt = DateTime.UtcNow;
                    productVariant.TenantId = product.TenantId;
                    productVariant.BranchId = product.BranchId;
                    productVariant.VariationValues = values;

                    await dbContext.ProductVariants.AddAsync(productVariant, cancellationToken);
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                product.Stock = await SumVariantStockAsync(product.Id, dbContext, cancellationToken);
                await EnsureStockRowsForProductVariantsAsync(product, dbContext, stockService, cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return product.Id;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private static async Task AddProductVariationOptionsAsync(
            int productId,
            CreateProductWithVariationsCommand request,
            IReadOnlyDictionary<string, Entities.VariationOption> optionByName,
            IApplicationDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var assigned = new HashSet<string>(StringComparer.Ordinal);
            foreach (var (optionDto, displayOrder) in request.VariationOptions.Select((dto, i) => (dto, i)))
            {
                if (!assigned.Add(optionDto.Name))
                    continue;

                var globalOption = optionByName[optionDto.Name];
                var pvo = new Entities.ProductVariationOption
                {
                    Title = globalOption.Name,
                    ProductId = productId,
                    VariationOptionId = globalOption.Id,
                    DisplayOrder = displayOrder,
                    CreatedAt = DateTime.UtcNow
                };
                await dbContext.ProductVariationOptions.AddAsync(pvo, cancellationToken);
            }
        }

        private static Task<int> SumVariantStockAsync(
            int productId,
            IApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
            dbContext.ProductVariants
                .Where(pv => pv.ProductId == productId)
                .SumAsync(pv => pv.Stock, cancellationToken);

        private static async Task EnsureStockRowsForProductVariantsAsync(
            Entities.Product product,
            IApplicationDbContext dbContext,
            IStockService stockService,
            CancellationToken cancellationToken)
        {
            var variantIds = await dbContext.ProductVariants
                .Where(pv => pv.ProductId == product.Id)
                .Select(pv => pv.Id)
                .ToListAsync(cancellationToken);

            foreach (var variantId in variantIds)
                await stockService.EnsureStockRecordExistsAsync(variantId, product.TenantId, cancellationToken);
        }
    }
}
