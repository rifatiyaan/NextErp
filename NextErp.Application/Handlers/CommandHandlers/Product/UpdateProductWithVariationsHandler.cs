using AutoMapper;
using NextErp.Application.Commands;
using NextErp.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Product
{
    public class UpdateProductWithVariationsHandler(
        IApplicationUnitOfWork unitOfWork,
        IApplicationDbContext dbContext,
        IMapper mapper)
        : IRequestHandler<UpdateProductWithVariationsCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateProductWithVariationsCommand request, CancellationToken cancellationToken)
        {
            // Begin transaction
            using var transaction = await dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted,
                cancellationToken);

            try
            {
                var product = await dbContext.Products
                    .Include(p => p.VariationOptions)
                        .ThenInclude(vo => vo.Values)
                    .Include(p => p.ProductVariants)
                        .ThenInclude(pv => pv.VariationValues)
                    .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

                if (product == null)
                {
                    throw new InvalidOperationException($"Product with ID {request.Id} not found.");
                }

                // 2. Update base product fields using AutoMapper
                mapper.Map(request, product);
                product.UpdatedAt = DateTime.UtcNow;

                // 3. Update or create variation options and values
                var existingOptions = product.VariationOptions.ToList();
                var optionMap = new Dictionary<string, Entities.VariationOption>(); // name -> option

                // Map existing options by name
                foreach (var existingOption in existingOptions)
                {
                    optionMap[existingOption.Name] = existingOption;
                }

                var allVariationValues = new List<Entities.VariationValue>();
                var valueKeyMap = new Dictionary<string, Entities.VariationValue>(); // "optionIndex:valueIndex" -> value

                // Process each option from request
                for (int optIdx = 0; optIdx < request.VariationOptions.Count; optIdx++)
                {
                    var optionDto = request.VariationOptions[optIdx];
                    Entities.VariationOption variationOption;

                    // Find or create option
                    if (optionMap.TryGetValue(optionDto.Name, out var existingOption))
                    {
                        // Update existing option using AutoMapper
                        mapper.Map(optionDto, existingOption);
                        existingOption.UpdatedAt = DateTime.UtcNow;
                        variationOption = existingOption;
                    }
                    else
                    {
                        // Create new option (shouldn't happen in edit mode, but handle it)
                        variationOption = mapper.Map<Entities.VariationOption>(optionDto);
                        variationOption.ProductId = product.Id;
                        variationOption.IsActive = true;
                        variationOption.CreatedAt = DateTime.UtcNow;
                        variationOption.TenantId = product.TenantId;
                        variationOption.BranchId = product.BranchId;
                        await dbContext.VariationOptions.AddAsync(variationOption, cancellationToken);
                        optionMap[optionDto.Name] = variationOption;
                    }

                    var existingValues = await dbContext.VariationValues
                        .AsNoTracking()
                        .Where(v => v.VariationOptionId == variationOption.Id && v.IsActive)
                        .ToListAsync(cancellationToken);

                    var existingValueMap = existingValues.ToDictionary(v => v.Value, v => v);

                    // Process each value from request
                    for (int valIdx = 0; valIdx < optionDto.Values.Count; valIdx++)
                    {
                        var valueDto = optionDto.Values[valIdx];
                        Entities.VariationValue variationValue;

                        // Find or create value
                        if (existingValueMap.TryGetValue(valueDto.Value, out var existingValue))
                        {
                            // Value already exists, use it
                            variationValue = existingValue;
                        }
                        else
                        {
                            // Create new value using AutoMapper
                            variationValue = mapper.Map<Entities.VariationValue>(valueDto);
                            variationValue.VariationOptionId = variationOption.Id;
                            variationValue.IsActive = true;
                            variationValue.CreatedAt = DateTime.UtcNow;
                            variationValue.TenantId = product.TenantId;
                            variationValue.BranchId = product.BranchId;
                        }

                        allVariationValues.Add(variationValue);
                        string key = $"{optIdx}:{valIdx}";
                        valueKeyMap[key] = variationValue;
                    }
                }

                // 4. Update or create product variants
                var existingVariants = product.ProductVariants.ToList();
                
                // Build variant lookup by variation value keys
                var existingVariantMap = new Dictionary<string, Entities.ProductVariant>();
                foreach (var variant in existingVariants)
                {
                    var keys = variant.VariationValues
                        .OrderBy(vv => vv.VariationOptionId)
                        .Select((vv, idx) => 
                        {
                            // Find the key for this value
                            foreach (var kvp in valueKeyMap)
                            {
                                if (kvp.Value.Id == vv.Id)
                                {
                                    return kvp.Key;
                                }
                            }
                            return null;
                        })
                        .Where(k => k != null)
                        .OrderBy(k => k)
                        .ToList();
                    
                    var keyString = string.Join(",", keys);
                    if (!string.IsNullOrEmpty(keyString))
                    {
                        existingVariantMap[keyString] = variant;
                    }
                }

                // Process each variant from request
                foreach (var variantDto in request.ProductVariants)
                {
                    var keys = variantDto.VariationValueKeys.OrderBy(k => k).ToList();
                    var keyString = string.Join(",", keys);

                    // Build variant title from variation values
                    var variantValueEntities = new List<Entities.VariationValue>();
                    foreach (var key in variantDto.VariationValueKeys)
                    {
                        if (valueKeyMap.TryGetValue(key, out var value))
                        {
                            variantValueEntities.Add(value);
                        }
                    }

                    var variantTitle = string.Join(" / ", variantValueEntities.Select(v => v.Value));

                    if (existingVariantMap.TryGetValue(keyString, out var existingVariant))
                    {
                        // Update existing variant using AutoMapper
                        mapper.Map(variantDto, existingVariant);
                        existingVariant.Title = variantTitle;
                        existingVariant.Name = variantTitle;
                        existingVariant.UpdatedAt = DateTime.UtcNow;
                        
                        // Update variation values if needed
                        var currentValueIds = existingVariant.VariationValues.Select(vv => vv.Id).OrderBy(id => id).ToList();
                        var newValueIds = variantValueEntities.Select(vv => vv.Id).OrderBy(id => id).ToList();
                        
                        if (!currentValueIds.SequenceEqual(newValueIds))
                        {
                            existingVariant.VariationValues.Clear();
                            foreach (var value in variantValueEntities)
                            {
                                existingVariant.VariationValues.Add(value);
                            }
                        }
                    }
                    else
                    {
                        // Create new variant using AutoMapper
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

                // 5. Save all changes
                await dbContext.SaveChangesAsync(cancellationToken);

                // 6. Commit transaction
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

