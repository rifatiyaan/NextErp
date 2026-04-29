using MediatR;
using NextErp.Application.Common.Attributes;
using NextErp.Application.Common.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Application.Commands;

[RequiresPermission("Stock.Adjust")]
public record CreateStockAdjustmentCommand(
    int ProductVariantId,
    StockAdjustmentMode Mode,
    decimal Quantity,
    string ReasonCode,
    string? Notes
) : IRequest<Guid>, ITransactionalRequest;
