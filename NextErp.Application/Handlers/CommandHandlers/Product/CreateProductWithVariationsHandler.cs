using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.DTOs;
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

                var optionNames = request.VariationOptions.Select(o => o.Name).Distinct().ToList();
                var globalOptions = await dbContext.VariationOptions
                    .Include(vo => vo.Values.OrderBy(v => v.DisplayOrder))
                    .Where(vo => vo.IsActive && optionNames.Contains(vo.Name))
                    .ToListAsync(cancellationToken);

                var optionByName = globalOptions.ToDictionary(vo => vo.Name, vo => vo);

                foreach (var (optionDto, displayOrder) in request.VariationOptions.Select((dto, i) => (dto, i)))
                {
                    if (!optionByName.TryGetValue(optionDto.Name, out var globalOption))
                        throw new InvalidOperationException($"Global variation option '{optionDto.Name}' not found. Create it first in Variation management.");

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

                foreach (var variantDto in request.ProductVariants)
                {
                    var variantValueEntities = variantDto.VariationValueKeys
                        .Select(key => valueKeyMap.GetValueOrDefault(key))
                        .Where(v => v != null)
                        .Cast<Entities.VariationValue>()
                        .ToList();

                    var variantTitle = string.Join(" / ", variantValueEntities.Select(v => v.Value));

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
    }
}
