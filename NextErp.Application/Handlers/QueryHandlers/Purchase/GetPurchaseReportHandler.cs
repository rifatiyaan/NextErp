using NextErp.Application.Queries;
using NextErp.Application.DTOs;
using MediatR;

namespace NextErp.Application.Handlers.QueryHandlers.Purchase
{
    public class GetPurchaseReportHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetPurchaseReportQuery, DTOs.Purchase.Response.Get.Report>
    {
        public async Task<DTOs.Purchase.Response.Get.Report> Handle(
            GetPurchaseReportQuery request,
            CancellationToken cancellationToken)
        {
            var purchases = await unitOfWork.PurchaseRepository.GetByDateRangeAsync(
                request.StartDate,
                request.EndDate);

            // Filter by supplier if specified
            if (request.SupplierId.HasValue)
            {
                purchases = purchases.Where(p => p.SupplierId == request.SupplierId.Value).ToList();
            }

            var purchaseDtos = purchases.Select(p => new DTOs.Purchase.Response.Get.Single
            {
                Id = p.Id,
                Title = p.Title,
                PurchaseNumber = p.PurchaseNumber,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier?.Title ?? "Unknown",
                PurchaseDate = p.PurchaseDate,
                TotalAmount = p.TotalAmount,
                Items = p.Items.Select(i => new DTOs.Purchase.Response.Get.PurchaseItemResponse
                {
                    Id = i.Id,
                    Title = i.Title,
                    ProductId = i.ProductId,
                    ProductTitle = i.Product?.Title ?? "Unknown",
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
            }).ToList();

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
