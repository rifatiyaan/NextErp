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
        IApplicationDbContext dbContext)
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
                // 1. Create base product
                var product = new Entities.Product
                {
                    Title = request.Title,
                    Code = request.Code,
                    ParentId = request.ParentId,
                    CategoryId = request.CategoryId,
                    Price = request.Price, // Base price (variants override)
                    Stock = request.Stock, // Base stock (variants override)
                    IsActive = request.IsActive,
                    ImageUrl = request.ImageUrl,
                    HasVariations = true, // Mark as having variations
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Entities.Product.ProductMetadataClass
                    {
                        Description = request.Description,
                        Color = request.Color,
                        Warranty = request.Warranty
                    }
                };

                await unitOfWork.ProductRepository.AddAsync(product);
                await unitOfWork.SaveAsync(); // Save to get product.Id

                // 2. Create VariationOptions and VariationValues
                var variationOptions = new List<Entities.VariationOption>();
                var allVariationValues = new List<Entities.VariationValue>();

                foreach (var optionDto in request.VariationOptions)
                {
                    var variationOption = new Entities.VariationOption
                    {
                        Title = optionDto.Name,
                        Name = optionDto.Name,
                        ProductId = product.Id,
                        DisplayOrder = optionDto.DisplayOrder,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        TenantId = product.TenantId,
                        BranchId = product.BranchId
                    };

                    variationOptions.Add(variationOption);
                }

                // Add all variation options
                foreach (var option in variationOptions)
                {
                    await dbContext.VariationOptions.AddAsync(option, cancellationToken);
                }
                await dbContext.SaveChangesAsync(cancellationToken); // Save to get option.Ids

                // 3. Create VariationValues for each option
                foreach (var (optionDto, optionIndex) in request.VariationOptions.Select((dto, idx) => (dto, idx)))
                {
                    var variationOption = variationOptions[optionIndex];

                    foreach (var (valueDto, valueIndex) in optionDto.Values.Select((dto, idx) => (dto, idx)))
                    {
                        var variationValue = new Entities.VariationValue
                        {
                            Title = valueDto.Value,
                            Name = valueDto.Value,
                            Value = valueDto.Value,
                            VariationOptionId = variationOption.Id,
                            DisplayOrder = valueDto.DisplayOrder,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            TenantId = product.TenantId,
                            BranchId = product.BranchId
                        };

                        allVariationValues.Add(variationValue);
                    }
                }

                // Add all variation values
                foreach (var value in allVariationValues)
                {
                    await dbContext.VariationValues.AddAsync(value, cancellationToken);
                }
                await dbContext.SaveChangesAsync(cancellationToken); // Save to get value.Ids

                // 4. Create ProductVariants
                // Build a map: "optionIndex:valueIndex" -> VariationValue entity
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

                    var productVariant = new Entities.ProductVariant
                    {
                        Title = variantTitle,
                        Name = variantTitle,
                        ProductId = product.Id,
                        Sku = variantDto.Sku,
                        Price = variantDto.Price,
                        Stock = variantDto.Stock,
                        IsActive = variantDto.IsActive,
                        CreatedAt = DateTime.UtcNow,
                        TenantId = product.TenantId,
                        BranchId = product.BranchId,
                        VariationValues = variantValueEntities
                    };

                    await dbContext.ProductVariants.AddAsync(productVariant, cancellationToken);
                }

                // 5. Save all changes
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

