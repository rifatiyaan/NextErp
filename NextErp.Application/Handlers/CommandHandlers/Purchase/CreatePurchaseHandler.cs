using NextErp.Application.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Purchase
{
    public class CreatePurchaseHandler(
        IApplicationUnitOfWork unitOfWork,
        IApplicationDbContext dbContext,
        IStockService stockService)
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
                var products = await unitOfWork.ProductRepository.Query()
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, cancellationToken);

                var purchaseNumber = string.IsNullOrWhiteSpace(request.PurchaseNumber)
                    ? $"PUR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
                    : request.PurchaseNumber;

                var purchase = new Entities.Purchase
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    PurchaseNumber = purchaseNumber,
                    SupplierId = request.SupplierId,
                    PurchaseDate = request.PurchaseDate,
                    TotalAmount = 0,
                    Discount = request.Discount,
                    Metadata = new Entities.Purchase.PurchaseMetadata
                    {
                        BatchNo = request.Metadata?.BatchNo,
                        BillNo = request.Metadata?.BillNo,
                        ChallanNo = request.Metadata?.ChallanNo,
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
                        Metadata = new Entities.PurchaseItem.PurchaseItemMetadata
                        {
                            Description = itemDto.Metadata?.Description,
                            Weight = itemDto.Metadata?.Weight,
                            ExpiryDate = itemDto.Metadata?.ExpiryDate,
                            BatchNumber = itemDto.Metadata?.BatchNumber
                        },
                        CreatedAt = DateTime.UtcNow,
                        TenantId = purchase.TenantId
                    };

                    purchase.Items.Add(item);
                    totalAmount += item.Total;

                    if (products.TryGetValue(itemDto.ProductId, out var product))
                    {
                        // Ensure stock record exists (creates if doesn't exist)
                        await stockService.EnsureStockRecordExistsAsync(itemDto.ProductId, purchase.TenantId, cancellationToken);
                        
                        // Increase stock (this will also update Product.Stock)
                        await stockService.IncreaseStockAsync(itemDto.ProductId, itemDto.Quantity, cancellationToken);
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
