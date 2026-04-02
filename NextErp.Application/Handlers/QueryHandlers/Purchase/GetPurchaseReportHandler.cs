using NextErp.Application;
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
                .Include(p => p.Party)
                .Include(p => p.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(pr => pr.Category)
                .Where(p => p.PurchaseDate >= request.StartDate && p.PurchaseDate <= request.EndDate);

            if (request.PartyId.HasValue)
            {
                query = query.Where(p => p.PartyId == request.PartyId.Value);
            }

            var purchaseDtos = await query
                .Select(p => new DTOs.Purchase.Response.Get.Single
                {
                    Id = p.Id,
                    Title = p.Title,
                    PurchaseNumber = p.PurchaseNumber,
                    PartyId = p.PartyId,
                    SupplierName = p.Party != null ? p.Party.Title : "Unknown",
                    PurchaseDate = p.PurchaseDate,
                    TotalAmount = p.TotalAmount,
                    Discount = p.Discount,
                    NetTotal = p.NetTotal,
                    Items = p.Items.Select(i => new DTOs.Purchase.Response.Get.PurchaseItemResponse
                    {
                        Id = i.Id,
                        Title = i.Title,
                        ProductVariantId = i.ProductVariantId,
                        ProductTitle = i.ProductVariant != null && i.ProductVariant.Product != null
                            ? i.ProductVariant.Product.Title
                            : "Unknown",
                        VariantSku = i.ProductVariant != null ? i.ProductVariant.Sku : "",
                        VariantTitle = i.ProductVariant != null ? i.ProductVariant.Title : "",
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
