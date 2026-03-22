using NextErp.Application.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Sale
{
    public class CreateSaleHandler(
        IApplicationUnitOfWork unitOfWork,
        IApplicationDbContext dbContext,
        IStockService stockService)
        : IRequestHandler<CreateSaleCommand, Guid>
    {
        public async Task<Guid> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted,
                cancellationToken);

            try
            {
                var variantIds = request.Items.Select(i => i.ProductVariantId).Distinct().ToList();
                var variants = await dbContext.ProductVariants
                    .Include(v => v.Product)
                    .Where(v => variantIds.Contains(v.Id))
                    .ToDictionaryAsync(v => v.Id, cancellationToken);

                if (variants.Count != variantIds.Count)
                {
                    var missing = variantIds.Except(variants.Keys).ToList();
                    throw new InvalidOperationException(
                        $"Product variant(s) not found: {string.Join(", ", missing)}.");
                }

                var tenantId = variants.Values.First().TenantId;

                foreach (var itemDto in request.Items)
                {
                    var variant = variants[itemDto.ProductVariantId];

                    await stockService.EnsureStockRecordExistsAsync(variant.Id, tenantId, cancellationToken);

                    var isAvailable = await stockService.CheckStockAvailabilityAsync(
                        variant.Id,
                        itemDto.Quantity,
                        cancellationToken);

                    if (isAvailable)
                        continue;

                    var available = await stockService.GetAvailableStockAsync(variant.Id, cancellationToken);
                    throw new InvalidOperationException(
                        $"Insufficient stock for SKU '{variant.Sku}' ({variant.Product?.Title}). " +
                        $"Available: {available}, Required: {itemDto.Quantity}.");
                }

                var saleNumber = $"SALE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

                var sale = new Entities.Sale
                {
                    Id = Guid.NewGuid(),
                    Title = $"Sale - {DateTime.UtcNow:yyyy-MM-dd HH:mm}",
                    SaleNumber = saleNumber,
                    CustomerId = request.CustomerId,
                    SaleDate = DateTime.UtcNow,
                    TotalAmount = request.TotalAmount,
                    Discount = request.Discount,
                    Tax = request.Tax,
                    FinalAmount = request.FinalAmount,
                    Metadata = new Entities.Sale.SaleMetadata
                    {
                        PaymentMethod = request.PaymentMethod
                    },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = null,
                    TenantId = tenantId
                };

                await unitOfWork.SaleRepository.AddAsync(sale);

                foreach (var itemDto in request.Items)
                {
                    var variant = variants[itemDto.ProductVariantId];
                    var lineTitle = $"{variant.Product?.Title ?? "Product"} — {variant.Title}";

                    var item = new Entities.SaleItem
                    {
                        Id = Guid.NewGuid(),
                        Title = lineTitle,
                        SaleId = sale.Id,
                        ProductVariantId = variant.Id,
                        Quantity = itemDto.Quantity,
                        Price = itemDto.Price,
                        CreatedAt = DateTime.UtcNow,
                        TenantId = sale.TenantId
                    };

                    sale.Items.Add(item);

                    await stockService.ReduceStockAsync(variant.Id, itemDto.Quantity, cancellationToken);
                }

                await unitOfWork.SaveAsync();
                await transaction.CommitAsync(cancellationToken);

                return sale.Id;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new InvalidOperationException("Stock was modified by another transaction. Please retry.");
            }
            catch (InvalidOperationException)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new InvalidOperationException($"Failed to create sale: {ex.Message}", ex);
            }
        }
    }
}
