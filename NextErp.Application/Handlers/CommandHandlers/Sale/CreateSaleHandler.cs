using AutoMapper;
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
            // Begin transaction with READ COMMITTED isolation level
            using var transaction = await dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted,
                cancellationToken);

            try
            {
                // 1. Check stock availability for all products FIRST
                foreach (var itemDto in request.Items)
                {
                    var stock = await unitOfWork.StockRepository.GetByIdAsync(itemDto.ProductId);
                    if (stock == null || stock.AvailableQuantity < itemDto.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient stock for product ID {itemDto.ProductId}. " +
                            $"Available: {stock?.AvailableQuantity ?? 0}, Required: {itemDto.Quantity}");
                    }
                }

                // 2. Create Sale master
                var sale = new Entities.Sale
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    SaleNumber = request.SaleNumber,
                    CustomerId = request.CustomerId,
                    SaleDate = request.SaleDate,
                    TotalAmount = 0, // Will be calculated
                    Metadata = new Entities.Sale.SaleMetadata
                    {
                        ReferenceNo = request.Metadata?.ReferenceNo,
                        PaymentMethod = request.Metadata?.PaymentMethod,
                        Notes = request.Metadata?.Notes
                    },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await unitOfWork.SaleRepository.AddAsync(sale);

                // 3. Create Sale items and calculate total
                decimal totalAmount = 0;
                foreach (var itemDto in request.Items)
                {
                    var item = new Entities.SaleItem
                    {
                        Id = Guid.NewGuid(),
                        Title = itemDto.Title,
                        SaleId = sale.Id,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        CreatedAt = DateTime.UtcNow,
                        TenantId = sale.TenantId
                    };

                    sale.Items.Add(item);
                    totalAmount += item.Total;

                    // 4. Decrease stock for each product
                    var stock = await unitOfWork.StockRepository.GetByIdAsync(itemDto.ProductId);
                    
                    // Double-check stock (defensive programming)
                    if (stock == null || stock.AvailableQuantity < itemDto.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Stock changed during transaction for product ID {itemDto.ProductId}");
                    }

                    stock.AvailableQuantity -= itemDto.Quantity;
                    stock.UpdatedAt = DateTime.UtcNow;
                    await unitOfWork.StockRepository.EditAsync(stock);
                }

                sale.TotalAmount = totalAmount;

                // 5. Save all changes (single SaveChanges call)
                await unitOfWork.SaveAsync();

                // 6. Commit transaction
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
