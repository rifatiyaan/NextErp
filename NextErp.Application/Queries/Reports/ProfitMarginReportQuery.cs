using MediatR;
using NextErp.Application.DTOs.Report;

namespace NextErp.Application.Queries.Reports;

/// <summary>
/// Per-sale revenue vs. product cost over a date range. Margin% is
/// (revenue - cost) / revenue * 100, computed at handler time so callers
/// don't have to re-derive it.
/// </summary>
public record ProfitMarginReportQuery(DateTime StartDate, DateTime EndDate)
    : IRequest<ProfitMarginResponse>;
