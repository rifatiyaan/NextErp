using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using NextErp.Application.Products;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Product
{
    public class UpdateProductWithVariationsHandler(
        IApplicationDbContext dbContext,
        IStockService stockService,
        IMapper mapper)
        : IRequestHandler<UpdateProductWithVariationsCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateProductWithVariationsCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted,
                cancellationToken);

            try
            {
                var product = await dbContext.Products
                    .Include(p => p.ProductVariationOptions)
                        .ThenInclude(pvo => pvo.VariationOption)
                        .ThenInclude(vo => vo.Values)
                    .Include(p => p.ProductVariants)
                        .ThenInclude(pv => pv.VariationValues)
                    .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

                if (product == null)
                    throw new InvalidOperationException($"Product with ID {request.Id} not found.");

                mapper.Map(request, product);
                product.UpdatedAt = DateTime.UtcNow;

                var optionByName = await ConfigurableProductVariantFactory.LoadActiveGlobalOptionsAsync(
                    dbContext,
                    request.VariationOptions.Select(o => o.Name),
                    cancellationToken);

                await SyncProductVariationOptionsAsync(product, request, optionByName, dbContext, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                if (request.ProductVariants.Count == 0)
                {
                    throw new InvalidOperationException(
                        "At least one product variant is required when saving variation options.");
                }

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

                var valueKeyMap = ConfigurableProductVariantFactory.BuildValueKeyMap(
                    request.VariationOptions,
                    optionByName);

                var valueIdToKey = ConfigurableProductVariantFactory.BuildVariationValueIdToKeyMap(valueKeyMap);
                var existingVariantMap = ConfigurableProductVariantFactory.IndexExistingVariantsByCombinationKey(
                    product.ProductVariants,
                    valueIdToKey);

                var requestedCombinationKeys = request.ProductVariants
                    .Select(d => ConfigurableProductVariantFactory.BuildCombinationKey(d.VariationValueKeys))
                    .ToHashSet(StringComparer.Ordinal);

                foreach (var variantDto in request.ProductVariants)
                {
                    var values = ConfigurableProductVariantFactory.ResolveVariationValues(
                        variantDto.VariationValueKeys,
                        valueKeyMap);

                    var title = ConfigurableProductVariantFactory.BuildDisplayTitle(values);
                    var combinationKey = ConfigurableProductVariantFactory.BuildCombinationKey(variantDto.VariationValueKeys);

                    if (existingVariantMap.TryGetValue(combinationKey, out var existingVariant))
                    {
                        mapper.Map(variantDto, existingVariant);
                        existingVariant.Title = title;
                        existingVariant.Name = title;
                        existingVariant.UpdatedAt = DateTime.UtcNow;
                        existingVariant.VariationValues.Clear();
                        foreach (var v in values)
                            existingVariant.VariationValues.Add(v);
                    }
                    else
                    {
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
                }

                DeactivateVariantsRemovedFromRequest(product, requestedCombinationKeys, valueIdToKey);

                await dbContext.SaveChangesAsync(cancellationToken);

                product.Stock = await dbContext.ProductVariants
                    .Where(pv => pv.ProductId == product.Id)
                    .SumAsync(pv => pv.Stock, cancellationToken);

                var variantIds = await dbContext.ProductVariants
                    .Where(pv => pv.ProductId == product.Id)
                    .Select(pv => pv.Id)
                    .ToListAsync(cancellationToken);

                foreach (var variantId in variantIds)
                    await stockService.EnsureStockRecordExistsAsync(variantId, product.TenantId, cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return Unit.Value;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private static async Task SyncProductVariationOptionsAsync(
            Entities.Product product,
            UpdateProductWithVariationsCommand request,
            IReadOnlyDictionary<string, Entities.VariationOption> optionByName,
            IApplicationDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var requestOptionNamesInOrder = request.VariationOptions.Select(o => o.Name).ToList();

            foreach (var pvo in product.ProductVariationOptions
                         .Where(pvo => !requestOptionNamesInOrder.Contains(pvo.VariationOption.Name))
                         .ToList())
            {
                dbContext.ProductVariationOptions.Remove(pvo);
            }

            var assignedNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var (optionDto, displayOrder) in request.VariationOptions.Select((dto, i) => (dto, i)))
            {
                if (!optionByName.TryGetValue(optionDto.Name, out var globalOption))
                {
                    throw new InvalidOperationException(
                        $"Global variation option '{optionDto.Name}' not found.");
                }

                if (!assignedNames.Add(optionDto.Name))
                    continue;

                var existingPvo = product.ProductVariationOptions
                    .FirstOrDefault(pvo => pvo.VariationOptionId == globalOption.Id);

                if (existingPvo != null)
                {
                    existingPvo.DisplayOrder = displayOrder;
                    continue;
                }

                var pvo = new Entities.ProductVariationOption
                {
                    Title = globalOption.Name,
                    ProductId = product.Id,
                    VariationOptionId = globalOption.Id,
                    DisplayOrder = displayOrder,
                    CreatedAt = DateTime.UtcNow
                };
                await dbContext.ProductVariationOptions.AddAsync(pvo, cancellationToken);
            }
        }

        private static void DeactivateVariantsRemovedFromRequest(
            Entities.Product product,
            HashSet<string> requestedCombinationKeys,
            IReadOnlyDictionary<int, string> valueIdToKey)
        {
            foreach (var v in product.ProductVariants)
            {
                var keys = v.VariationValues
                    .Select(vv => valueIdToKey.GetValueOrDefault(vv.Id))
                    .Where(k => !string.IsNullOrEmpty(k))
                    .Cast<string>();

                var combinationKey = ConfigurableProductVariantFactory.BuildCombinationKey(keys);
                if (string.IsNullOrEmpty(combinationKey) || !requestedCombinationKeys.Contains(combinationKey))
                    v.IsActive = false;
            }
        }
    }
}
