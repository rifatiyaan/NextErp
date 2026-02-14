using NextErp.Application.Queries;
using NextErp.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Application.Handlers.QueryHandlers.Purchase
{
    public class GetPurchaseReportHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetPurchaseReportQuery, DTOs.Purchase.Response.Get.Report>
    {
        public async Task<DTOs.Purchase.Response.Get.Report> Handle(
            GetPurchaseReportQuery request,
            CancellationToken cancellationToken)
        {
            var query = unitOfWork.PurchaseRepository.Query()
                .AsNoTracking()
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(pr => pr.Category)
                .Where(p => p.PurchaseDate >= request.StartDate && p.PurchaseDate <= request.EndDate);

            if (request.SupplierId.HasValue)
            {
                query = query.Where(p => p.SupplierId == request.SupplierId.Value);
            }

            var purchaseDtos = await query
                .Select(p => new DTOs.Purchase.Response.Get.Single
                {
                    Id = p.Id,
                    Title = p.Title,
                    PurchaseNumber = p.PurchaseNumber,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier != null ? p.Supplier.Title : "Unknown",
                    PurchaseDate = p.PurchaseDate,
                    TotalAmount = p.TotalAmount,
                    Items = p.Items.Select(i => new DTOs.Purchase.Response.Get.PurchaseItemResponse
                    {
                        Id = i.Id,
                        Title = i.Title,
                        ProductId = i.ProductId,
                        ProductTitle = i.Product != null ? i.Product.Title : "Unknown",
                        Quantity = i.Quantity,
                        UnitCost = i.UnitCost,
                        Total = i.Total
                    }).ToList(),
                    Metadata = new DTOs.Purchase.Request.Metadata
                    {
                        ReferenceNo = p.Metadata.ReferenceNo,
                        Notes = p.Metadata.Notes
                    },
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    TenantId = p.TenantId,
                    BranchId = p.BranchId
                })
                .ToListAsync(cancellationToken);

            return new DTOs.Purchase.Response.Get.Report
            {
                Purchases = purchaseDtos,
                TotalPurchaseAmount = purchaseDtos.Sum(p => p.TotalAmount),
                TotalPurchases = purchaseDtos.Count,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };
        }
    }
}
