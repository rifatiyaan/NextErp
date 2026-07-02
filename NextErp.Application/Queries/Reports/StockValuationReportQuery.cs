using MediatR;
using NextErp.Application.DTOs.Report;

namespace NextErp.Application.Queries.Reports;

/// <summary>
/// Snapshot of inventory value at a given moment. Aggregates per-product
/// quantity × cost across all stock rows in the current branch scope.
/// </summary>
/// <param name="AsOf">
/// Cut-off timestamp. Currently informational (used as the report header)
/// because Stock holds the current snapshot — full point-in-time valuation
/// would need to replay StockMovement deltas, which we defer to a follow-up.
/// </param>
public record StockValuationReportQuery(DateTime AsOf)
    : IRequest<StockValuationResponse>;
