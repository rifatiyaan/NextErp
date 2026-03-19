using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Product
{
    public class UpdateProductWithVariationsHandler(
        IApplicationDbContext dbContext,
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

                var optionNames = request.VariationOptions.Select(o => o.Name).Distinct().ToList();
                var globalOptions = await dbContext.VariationOptions
                    .Include(vo => vo.Values.OrderBy(v => v.DisplayOrder))
                    .Where(vo => vo.IsActive && optionNames.Contains(vo.Name))
                    .ToListAsync(cancellationToken);

                var optionByName = globalOptions.ToDictionary(vo => vo.Name, vo => vo);

                var existingPvos = product.ProductVariationOptions.ToList();
                var requestOptionNamesInOrder = request.VariationOptions.Select(o => o.Name).ToList();

                foreach (var pvo in existingPvos.Where(pvo => !requestOptionNamesInOrder.Contains(pvo.VariationOption.Name)))
                    dbContext.ProductVariationOptions.Remove(pvo);

                var assignedNames = new HashSet<string>();
                foreach (var (optionDto, displayOrder) in request.VariationOptions.Select((dto, i) => (dto, i)))
                {
                    if (!optionByName.TryGetValue(optionDto.Name, out var globalOption))
                        throw new InvalidOperationException($"Global variation option '{optionDto.Name}' not found.");

                    if (assignedNames.Add(optionDto.Name))
                    {
                        var alreadyAssigned = product.ProductVariationOptions.Any(pvo => pvo.VariationOptionId == globalOption.Id);
                        if (!alreadyAssigned)
                        {
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
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                var valueKeyMap = new Dictionary<string, Entities.VariationValue>();
                foreach (var (optionDto, optIdx) in request.VariationOptions.Select((dto, idx) => (dto, idx)))
                {
                    if (!optionByName.TryGetValue(optionDto.Name, out var globalOption))
                        continue;
                    var valuesInOrder = globalOption.Values.Where(v => v.IsActive).OrderBy(v => v.DisplayOrder).ToList();
                    foreach (var (val, valIdx) in valuesInOrder.Select((v, i) => (v, i)))
                        valueKeyMap[$"{optIdx}:{valIdx}"] = val;
                }

                var valueIdToKey = valueKeyMap.ToDictionary(kvp => kvp.Value.Id, kvp => kvp.Key);
                var existingVariants = product.ProductVariants.ToList();
                var existingVariantMap = existingVariants
                    .Select(v => (Variant: v, KeyString: string.Join(",", v.VariationValues.Select(vv => valueIdToKey.GetValueOrDefault(vv.Id)).Where(k => k != null).OrderBy(k => k))))
                    .Where(x => !string.IsNullOrEmpty(x.KeyString))
                    .ToDictionary(x => x.KeyString, x => x.Variant);

                foreach (var variantDto in request.ProductVariants)
                {
                    var variantValueEntities = variantDto.VariationValueKeys
                        .Select(key => valueKeyMap.GetValueOrDefault(key))
                        .Where(v => v != null)
                        .Cast<Entities.VariationValue>()
                        .ToList();

                    var variantTitle = string.Join(" / ", variantValueEntities.Select(v => v.Value));
                    var keyString = string.Join(",", variantDto.VariationValueKeys.OrderBy(k => k));

                    if (existingVariantMap.TryGetValue(keyString, out var existingVariant))
                    {
                        mapper.Map(variantDto, existingVariant);
                        existingVariant.Title = variantTitle;
                        existingVariant.Name = variantTitle;
                        existingVariant.UpdatedAt = DateTime.UtcNow;
                        existingVariant.VariationValues.Clear();
                        foreach (var v in variantValueEntities)
                            existingVariant.VariationValues.Add(v);
                    }
                    else
                    {
                        var productVariant = mapper.Map<Entities.ProductVariant>(variantDto);
                        productVariant.Title = variantTitle;
                        productVariant.Name = variantTitle;
                        productVariant.ProductId = product.Id;
                        productVariant.CreatedAt = DateTime.UtcNow;
                        productVariant.TenantId = product.TenantId;
                        productVariant.BranchId = product.BranchId;
                        productVariant.VariationValues = variantValueEntities;
                        await dbContext.ProductVariants.AddAsync(productVariant, cancellationToken);
                    }
                }

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
    }
}
