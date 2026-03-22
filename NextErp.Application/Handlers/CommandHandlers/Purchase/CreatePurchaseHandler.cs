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

                var purchaseNumber = string.IsNullOrWhiteSpace(request.PurchaseNumber)
                    ? $"PUR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
                    : request.PurchaseNumber;

                var tenantId = variants.Values.First().TenantId;

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
                    CreatedAt = DateTime.UtcNow,
                    TenantId = tenantId
                };

                await unitOfWork.PurchaseRepository.AddAsync(purchase);

                decimal totalAmount = 0;
                foreach (var itemDto in request.Items)
                {
                    var variant = variants[itemDto.ProductVariantId];
                    var lineTitle = string.IsNullOrWhiteSpace(itemDto.Title)
                        ? $"{variant.Product?.Title ?? "Product"} — {variant.Title}"
                        : itemDto.Title;

                    var item = new Entities.PurchaseItem
                    {
                        Id = Guid.NewGuid(),
                        Title = lineTitle,
                        PurchaseId = purchase.Id,
                        ProductVariantId = variant.Id,
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

                    await stockService.EnsureStockRecordExistsAsync(variant.Id, purchase.TenantId, cancellationToken);
                    await stockService.IncreaseStockAsync(variant.Id, itemDto.Quantity, cancellationToken);
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
