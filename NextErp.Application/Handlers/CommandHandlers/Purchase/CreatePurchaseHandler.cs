using AutoMapper;
using NextErp.Application.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Purchase
{
    public class CreatePurchaseHandler(
        IApplicationUnitOfWork unitOfWork,
        IApplicationDbContext dbContext)
        : IRequestHandler<CreatePurchaseCommand, Guid>
    {
        public async Task<Guid> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
        {
            // Begin transaction with READ COMMITTED isolation level
            using var transaction = await dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted,
                cancellationToken);

            try
            {
                // 1. Create Purchase master
                var purchase = new Entities.Purchase
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    PurchaseNumber = request.PurchaseNumber,
                    SupplierId = request.SupplierId,
                    PurchaseDate = request.PurchaseDate,
                    TotalAmount = 0, // Will be calculated
                    Metadata = new Entities.Purchase.PurchaseMetadata
                    {
                        ReferenceNo = request.Metadata?.ReferenceNo,
                        Notes = request.Metadata?.Notes
                    },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await unitOfWork.PurchaseRepository.AddAsync(purchase);

                // 2. Create Purchase items and calculate total
                decimal totalAmount = 0;
                foreach (var itemDto in request.Items)
                {
                    var item = new Entities.PurchaseItem
                    {
                        Id = Guid.NewGuid(),
                        Title = itemDto.Title,
                        PurchaseId = purchase.Id,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitCost = itemDto.UnitCost,
                        CreatedAt = DateTime.UtcNow,
                        TenantId = purchase.TenantId
                    };

                    purchase.Items.Add(item);
                    totalAmount += item.Total;

                    // 3. Update stock for each product
                    var stock = await unitOfWork.StockRepository.GetByIdAsync(itemDto.ProductId);
                    if (stock == null)
                    {
                        // Create stock if doesn't exist
                        stock = new Entities.Stock
                        {
                            Id = itemDto.ProductId,
                            ProductId = itemDto.ProductId,
                            AvailableQuantity = itemDto.Quantity,
                            CreatedAt = DateTime.UtcNow,
                            TenantId = purchase.TenantId,
                            BranchId = purchase.BranchId
                        };
                        await unitOfWork.StockRepository.AddAsync(stock);
                    }
                    else
                    {
                        // Increase stock quantity
                        stock.AvailableQuantity += itemDto.Quantity;
                        stock.UpdatedAt = DateTime.UtcNow;
                        await unitOfWork.StockRepository.EditAsync(stock);
                    }
                }

                purchase.TotalAmount = totalAmount;

                // 4. Save all changes (single SaveChanges call)
                await unitOfWork.SaveAsync();

                // 5. Commit transaction
                await transaction.CommitAsync(cancellationToken);

                return purchase.Id;
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
