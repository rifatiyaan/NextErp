using NextErp.Application;
using NextErp.Application.Queries;
using NextErp.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Application.Handlers.QueryHandlers.Sale
{
    public class GetSalesReportHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetSalesReportQuery, DTOs.Sale.Response.Get.Report>
    {
        public async Task<DTOs.Sale.Response.Get.Report> Handle(
            GetSalesReportQuery request,
            CancellationToken cancellationToken)
        {
            var query = unitOfWork.SaleRepository.Query()
                .AsNoTracking()
                .Include(s => s.Party)
                .Include(s => s.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(s => s.Payments)
                .Where(s => s.SaleDate >= request.StartDate && s.SaleDate <= request.EndDate);

            if (request.PartyId.HasValue)
            {
                query = query.Where(s => s.PartyId == request.PartyId.Value);
            }

            var saleDtos = await query
                .Select(s => new DTOs.Sale.Response.Get.Single
                {
                    Id = s.Id,
                    Title = s.Title,
                    SaleNumber = s.SaleNumber,
                    PartyId = s.PartyId,
                    CustomerName = s.Party != null ? s.Party.Title : "Unknown",
                    SaleDate = s.SaleDate,
                    TotalAmount = s.TotalAmount,
                    Discount = s.Discount,
                    Tax = s.Tax,
                    FinalAmount = s.FinalAmount,
                    TotalPaid = s.Payments.Sum(p => p.Amount),
                    BalanceDue = s.FinalAmount - s.Payments.Sum(p => p.Amount),
                    Items = s.Items.Select(i => new DTOs.Sale.Response.Get.SaleItemResponse
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
                        UnitPrice = i.UnitPrice,
                        Total = i.Total
                    }).ToList(),
                    Payments = s.Payments
                        .OrderBy(p => p.PaidAt)
                        .ThenBy(p => p.CreatedAt)
                        .Select(p => new DTOs.Payment.Response.Line
                        {
                            Id = p.Id,
                            SaleId = p.SaleId,
                            Amount = p.Amount,
                            PaymentMethod = p.PaymentMethod,
                            PaidAt = p.PaidAt,
                            Reference = p.Reference,
                            CreatedAt = p.CreatedAt
                        })
                        .ToList(),
                    Metadata = new DTOs.Sale.Request.Metadata
                    {
                        ReferenceNo = s.Metadata.ReferenceNo,
                        PaymentMethod = s.Metadata.PaymentMethod,
                        Notes = s.Metadata.Notes
                    },
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    TenantId = s.TenantId,
                    BranchId = s.BranchId
                })
                .ToListAsync(cancellationToken);

            return new DTOs.Sale.Response.Get.Report
            {
                Sales = saleDtos,
                TotalSalesAmount = saleDtos.Sum(s => s.TotalAmount),
                TotalSales = saleDtos.Count,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };
        }
    }
}
