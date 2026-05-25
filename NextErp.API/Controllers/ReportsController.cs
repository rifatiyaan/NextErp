using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using NextErp.Application.Queries.Reports;

namespace NextErp.API.Controllers;

/// <summary>
/// PDF report endpoints. All return <c>application/pdf</c> with
/// <c>Content-Disposition: attachment</c> by default so the browser
/// triggers a save dialog — pass <c>?inline=true</c> to embed in an iframe.
/// </summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ReportsController(
    IMediator mediator,
    IReportPdfService reportPdfService) : ControllerBase
{
    // GET api/reports/sales.pdf?from=2026-01-01&to=2026-01-31
    [HttpGet("sales.pdf")]
    public async Task<IActionResult> SalesReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] Guid? partyId = null,
        [FromQuery] bool inline = false,
        CancellationToken ct = default)
    {
        if (to < from)
            return BadRequest(new { detail = "`to` must be on or after `from`." });

        var report = await mediator.Send(new GetSalesReportQuery(from, to, partyId), ct);
        var bytes = await reportPdfService.RenderSalesReportAsync(report, ct);
        return PdfFile(bytes, $"sales-report-{from:yyyyMMdd}-{to:yyyyMMdd}.pdf", inline);
    }

    // GET api/reports/stock-valuation.pdf
    [HttpGet("stock-valuation.pdf")]
    public async Task<IActionResult> StockValuation(
        [FromQuery] bool inline = false,
        CancellationToken ct = default)
    {
        var report = await mediator.Send(new StockValuationReportQuery(DateTime.UtcNow), ct);
        var bytes = await reportPdfService.RenderStockValuationReportAsync(report, ct);
        return PdfFile(bytes, $"stock-valuation-{DateTime.UtcNow:yyyyMMdd-HHmm}.pdf", inline);
    }

    // GET api/reports/profit-margin.pdf?from=2026-01-01&to=2026-01-31
    [HttpGet("profit-margin.pdf")]
    public async Task<IActionResult> ProfitMargin(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] bool inline = false,
        CancellationToken ct = default)
    {
        if (to < from)
            return BadRequest(new { detail = "`to` must be on or after `from`." });

        var report = await mediator.Send(new ProfitMarginReportQuery(from, to), ct);
        var bytes = await reportPdfService.RenderProfitMarginReportAsync(report, ct);
        return PdfFile(bytes, $"profit-margin-{from:yyyyMMdd}-{to:yyyyMMdd}.pdf", inline);
    }

    // ---- JSON endpoints (same data as the PDFs, but shaped for the
    //      list-format report pages on the frontend) ------------------------

    // GET api/reports/sales
    //
    // JSON variant of the sales report. Same query as the PDF endpoint —
    // splitting the route keeps Content-Type-based content-negotiation
    // simple (the frontend table consumer needs JSON; the iframe wants
    // application/pdf). Returns Sale.Response.Get.Report directly so the
    // shape matches the existing OpenAPI contract used by other consumers.
    [HttpGet("sales")]
    public async Task<IActionResult> SalesReportData(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] Guid? partyId = null,
        CancellationToken ct = default)
    {
        if (to < from)
            return BadRequest(new { detail = "`to` must be on or after `from`." });

        var report = await mediator.Send(new GetSalesReportQuery(from, to, partyId), ct);
        return Ok(report);
    }

    [HttpGet("stock-valuation")]
    public async Task<IActionResult> StockValuationData(CancellationToken ct = default)
    {
        var report = await mediator.Send(new StockValuationReportQuery(DateTime.UtcNow), ct);
        return Ok(report);
    }

    [HttpGet("profit-margin")]
    public async Task<IActionResult> ProfitMarginData(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct = default)
    {
        if (to < from)
            return BadRequest(new { detail = "`to` must be on or after `from`." });

        var report = await mediator.Send(new ProfitMarginReportQuery(from, to), ct);
        return Ok(report);
    }

    // ---- helper -------------------------------------------------------------

    /// <summary>
    /// Returns a PDF FileResult with disposition flipped between attachment
    /// (force download) and inline (browser embed). All three report routes
    /// follow the same convention so the frontend can reuse a single
    /// fetch-blob helper.
    /// </summary>
    private FileResult PdfFile(byte[] bytes, string fileName, bool inline)
    {
        var disposition = inline ? "inline" : "attachment";
        Response.Headers["Content-Disposition"] = $"{disposition}; filename=\"{fileName}\"";
        return File(bytes, "application/pdf");
    }
}
