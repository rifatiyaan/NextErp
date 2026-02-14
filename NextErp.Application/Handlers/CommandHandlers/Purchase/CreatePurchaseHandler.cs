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
            using var transaction = await dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted,
                cancellationToken);

            try
            {
                var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
                var stocks = await unitOfWork.StockRepository.Query()
                    .Where(s => productIds.Contains(s.ProductId))
                    .ToDictionaryAsync(s => s.ProductId, cancellationToken);

                var purchase = new Entities.Purchase
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    PurchaseNumber = request.PurchaseNumber,
                    SupplierId = request.SupplierId,
                    PurchaseDate = request.PurchaseDate,
                    TotalAmount = 0,
                    Metadata = new Entities.Purchase.PurchaseMetadata
                    {
                        ReferenceNo = request.Metadata?.ReferenceNo,
                        Notes = request.Metadata?.Notes
                    },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await unitOfWork.PurchaseRepository.AddAsync(purchase);

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

                    if (!stocks.TryGetValue(itemDto.ProductId, out var stock))
                    {
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
                        stocks[itemDto.ProductId] = stock;
                    }
                    else
                    {
                        stock.AvailableQuantity += itemDto.Quantity;
                        stock.UpdatedAt = DateTime.UtcNow;
                    }
                }

                purchase.TotalAmount = totalAmount;
                await unitOfWork.SaveAsync();
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
