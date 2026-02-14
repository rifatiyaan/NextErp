using AutoMapper;
using NextErp.Application.Commands;
using NextErp.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Product
{
    public class CreateProductWithVariationsHandler(
        IApplicationUnitOfWork unitOfWork,
        IApplicationDbContext dbContext,
        IMapper mapper)
        : IRequestHandler<CreateProductWithVariationsCommand, int>
    {
        public async Task<int> Handle(CreateProductWithVariationsCommand request, CancellationToken cancellationToken)
        {
            // Begin transaction
            using var transaction = await dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted,
                cancellationToken);

            try
            {
                // 1. Create base product using AutoMapper
                var product = mapper.Map<Entities.Product>(request);
                product.HasVariations = true;
                product.CreatedAt = DateTime.UtcNow;

                await unitOfWork.ProductRepository.AddAsync(product);
                await unitOfWork.SaveAsync(); // Save to get product.Id

                // 2. Create VariationOptions and VariationValues
                var variationOptions = new List<Entities.VariationOption>();
                var allVariationValues = new List<Entities.VariationValue>();

                foreach (var optionDto in request.VariationOptions)
                {
                    var variationOption = mapper.Map<Entities.VariationOption>(optionDto);
                    variationOption.ProductId = product.Id;
                    variationOption.IsActive = true;
                    variationOption.CreatedAt = DateTime.UtcNow;
                    variationOption.TenantId = product.TenantId;
                    variationOption.BranchId = product.BranchId;

                    variationOptions.Add(variationOption);
                }

                await dbContext.VariationOptions.AddRangeAsync(variationOptions, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                foreach (var (optionDto, optionIndex) in request.VariationOptions.Select((dto, idx) => (dto, idx)))
                {
                    var variationOption = variationOptions[optionIndex];

                    foreach (var valueDto in optionDto.Values)
                    {
                        var variationValue = mapper.Map<Entities.VariationValue>(valueDto);
                        variationValue.VariationOptionId = variationOption.Id;
                        variationValue.IsActive = true;
                        variationValue.CreatedAt = DateTime.UtcNow;
                        variationValue.TenantId = product.TenantId;
                        variationValue.BranchId = product.BranchId;

                        allVariationValues.Add(variationValue);
                    }
                }

                await dbContext.VariationValues.AddRangeAsync(allVariationValues, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                var valueKeyMap = new Dictionary<string, Entities.VariationValue>();
                int optIdx = 0;
                foreach (var optionDto in request.VariationOptions)
                {
                    int valIdx = 0;
                    var option = variationOptions[optIdx];
                    var optionValues = allVariationValues.Where(v => v.VariationOptionId == option.Id).ToList();
                    
                    foreach (var value in optionValues)
                    {
                        string key = $"{optIdx}:{valIdx}";
                        valueKeyMap[key] = value;
                        valIdx++;
                    }
                    optIdx++;
                }

                var productVariants = new List<Entities.ProductVariant>();
                foreach (var variantDto in request.ProductVariants)
                {
                    // Build variant title from variation values using keys
                    var variantValueEntities = new List<Entities.VariationValue>();
                    foreach (var key in variantDto.VariationValueKeys)
                    {
                        if (valueKeyMap.TryGetValue(key, out var value))
                        {
                            variantValueEntities.Add(value);
                        }
                    }

                    var variantTitle = string.Join(" / ", variantValueEntities.Select(v => v.Value));

                    var productVariant = mapper.Map<Entities.ProductVariant>(variantDto);
                    productVariant.Title = variantTitle;
                    productVariant.Name = variantTitle;
                    productVariant.ProductId = product.Id;
                    productVariant.CreatedAt = DateTime.UtcNow;
                    productVariant.TenantId = product.TenantId;
                    productVariant.BranchId = product.BranchId;
                    productVariant.VariationValues = variantValueEntities;

                    productVariants.Add(productVariant);
                }

                await dbContext.ProductVariants.AddRangeAsync(productVariants, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                // 6. Commit transaction
                await transaction.CommitAsync(cancellationToken);

                return product.Id;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}

