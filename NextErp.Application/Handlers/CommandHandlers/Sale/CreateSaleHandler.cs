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
                var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
                var products = await unitOfWork.ProductRepository.Query()
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, cancellationToken);

                var tenantId = Guid.Empty;

                foreach (var itemDto in request.Items)
                {
                    if (!products.TryGetValue(itemDto.ProductId, out var product))
                    {
                        throw new InvalidOperationException($"Product with ID {itemDto.ProductId} not found.");
                    }

                    tenantId = product.TenantId;

                    await stockService.EnsureStockRecordExistsAsync(itemDto.ProductId, tenantId, cancellationToken);

                    var isAvailable = await stockService.CheckStockAvailabilityAsync(
                        itemDto.ProductId,
                        itemDto.Quantity,
                        cancellationToken);

                    if (!isAvailable)
                    {
                        var availableStock = await stockService.GetAvailableStockAsync(
                            itemDto.ProductId,
                            cancellationToken);
                        throw new InvalidOperationException(
                            $"Insufficient stock for product '{product.Title}'. " +
                            $"Available: {availableStock}, Required: {itemDto.Quantity}");
                    }
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
                    var product = products[itemDto.ProductId];

                    var item = new Entities.SaleItem
                    {
                        Id = Guid.NewGuid(),
                        Title = product.Title,
                        SaleId = sale.Id,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        Price = itemDto.Price,
                        CreatedAt = DateTime.UtcNow,
                        TenantId = sale.TenantId
                    };

                    sale.Items.Add(item);

                    await stockService.ReduceStockAsync(itemDto.ProductId, itemDto.Quantity, cancellationToken);
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
