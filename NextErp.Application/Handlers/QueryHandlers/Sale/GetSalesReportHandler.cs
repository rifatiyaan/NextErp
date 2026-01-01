using NextErp.Application.Queries;
using NextErp.Application.DTOs;
using MediatR;

namespace NextErp.Application.Handlers.QueryHandlers.Sale
{
    public class GetSalesReportHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetSalesReportQuery, DTOs.Sale.Response.Get.Report>
    {
        public async Task<DTOs.Sale.Response.Get.Report> Handle(
            GetSalesReportQuery request,
            CancellationToken cancellationToken)
        {
            var sales = await unitOfWork.SaleRepository.GetByDateRangeAsync(
                request.StartDate,
                request.EndDate);

            // Filter by customer if specified
            if (request.CustomerId.HasValue)
            {
                sales = sales.Where(s => s.CustomerId == request.CustomerId.Value).ToList();
            }

            var saleDtos = sales.Select(s => new DTOs.Sale.Response.Get.Single
            {
                Id = s.Id,
                Title = s.Title,
                SaleNumber = s.SaleNumber,
                CustomerId = s.CustomerId,
                CustomerName = s.Customer?.Title ?? "Unknown",
                SaleDate = s.SaleDate,
                TotalAmount = s.TotalAmount,
                Items = s.Items.Select(i => new DTOs.Sale.Response.Get.SaleItemResponse
                {
                    Id = i.Id,
                    Title = i.Title,
                    ProductId = i.ProductId,
                    ProductTitle = i.Product?.Title ?? "Unknown",
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
            }).ToList();

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
