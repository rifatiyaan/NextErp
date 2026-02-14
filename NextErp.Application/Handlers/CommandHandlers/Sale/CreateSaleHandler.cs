using NextErp.Application.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Sale
{
    public class CreateSaleHandler(
        IApplicationUnitOfWork unitOfWork,
        IApplicationDbContext dbContext)
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

                foreach (var itemDto in request.Items)
                {
                    if (!products.TryGetValue(itemDto.ProductId, out var product))
                    {
                        throw new InvalidOperationException($"Product with ID {itemDto.ProductId} not found.");
                    }
                    
                    if (product.Stock < (int)itemDto.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient stock for product {product.Title}. " +
                            $"Available: {product.Stock}, Required: {itemDto.Quantity}");
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
                    CreatedBy = null
                };

                await unitOfWork.SaleRepository.AddAsync(sale);

                foreach (var itemDto in request.Items)
                {
                    var product = products[itemDto.ProductId];
                    
                    if (product.Stock < (int)itemDto.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Stock changed during transaction for product ID {itemDto.ProductId}");
                    }

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

                    product.Stock -= (int)itemDto.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
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
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
