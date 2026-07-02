using NextErp.Application.Common.Extensions;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SaleDto = NextErp.Application.DTOs.Sale;
using PaymentDto = NextErp.Application.DTOs.Payment;

namespace NextErp.Application.Handlers.QueryHandlers.Sale
{
    public class GetSalesReportHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetSalesReportQuery, SaleDto.SaleReportResponse>
    {
        public async Task<SaleDto.SaleReportResponse> Handle(
            GetSalesReportQuery request,
            CancellationToken cancellationToken = default)
        {
            var query = dbContext.Sales
                .AsNoTracking()
                .Include(s => s.Party)
                .Include(s => s.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(s => s.Payments)
                .Where(s => s.SaleDate >= request.StartDate && s.SaleDate <= request.EndDate)
                .WhereIfHasValue(request.PartyId, s => s.PartyId == request.PartyId!.Value);

            var saleDtos = await query
                .Select(s => new SaleDto.SaleResponse
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
                    Items = s.Items.Select(i => new SaleDto.SaleItemResponse
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
                        Discount = i.Discount,
                        DiscountSource = i.DiscountSource.HasValue ? i.DiscountSource.Value.ToString() : null,
                        PromotionId = i.PromotionId,
                        Total = i.Total
                    }).ToList(),
                    Payments = s.Payments
                        .OrderBy(p => p.PaidAt)
                        .ThenBy(p => p.CreatedAt)
                        .Select(p => new PaymentDto.PaymentLineResponse
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
                    Metadata = new SaleDto.SaleMetadataRequest
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

            return new SaleDto.SaleReportResponse
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
