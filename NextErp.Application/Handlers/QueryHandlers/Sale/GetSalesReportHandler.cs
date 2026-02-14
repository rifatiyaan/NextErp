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
                .Include(s => s.Customer)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Product)
                .Where(s => s.SaleDate >= request.StartDate && s.SaleDate <= request.EndDate);

            if (request.CustomerId.HasValue)
            {
                query = query.Where(s => s.CustomerId == request.CustomerId.Value);
            }

            var saleDtos = await query
                .Select(s => new DTOs.Sale.Response.Get.Single
                {
                    Id = s.Id,
                    Title = s.Title,
                    SaleNumber = s.SaleNumber,
                    CustomerId = s.CustomerId ?? Guid.Empty,
                    CustomerName = s.Customer != null ? s.Customer.Title : "Unknown",
                    SaleDate = s.SaleDate,
                    TotalAmount = s.TotalAmount,
                    Items = s.Items.Select(i => new DTOs.Sale.Response.Get.SaleItemResponse
                    {
                        Id = i.Id,
                        Title = i.Title,
                        ProductId = i.ProductId,
                        ProductTitle = i.Product != null ? i.Product.Title : "Unknown",
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Total = i.Total
                    }).ToList(),
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
