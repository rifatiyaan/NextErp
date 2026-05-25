using SaleDto = NextErp.Application.DTOs.Sale;
using ReportDto = NextErp.Application.DTOs.Report;

namespace NextErp.Application.Interfaces;

/// <summary>
/// Renders the three Phase B reports to PDF: sales summary, stock
/// valuation, and profit margin. Single service with three methods rather
/// than three services because they share layout primitives (header,
/// summary block, table, footer) — split would duplicate that code.
/// </summary>
public interface IReportPdfService
{
    /// <summary>Sales report PDF over a date range. Uses the existing GetSalesReport DTO.</summary>
    Task<byte[]> RenderSalesReportAsync(
        SaleDto.Response.Get.Report report,
        CancellationToken cancellationToken = default);

    /// <summary>Inventory valuation snapshot — quantity × cost across the catalog.</summary>
    Task<byte[]> RenderStockValuationReportAsync(
        ReportDto.Response.StockValuation report,
        CancellationToken cancellationToken = default);

    /// <summary>Profit margin per sale across a date range.</summary>
    Task<byte[]> RenderProfitMarginReportAsync(
        ReportDto.Response.ProfitMargin report,
        CancellationToken cancellationToken = default);
}
