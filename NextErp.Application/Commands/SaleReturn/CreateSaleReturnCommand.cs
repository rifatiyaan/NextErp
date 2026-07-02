using MediatR;
using NextErp.Application.Common.Interfaces;
using NextErp.Application.DTOs.Returns;

namespace NextErp.Application.Commands.SaleReturn;

/// <summary>
/// Creates a Sale Return + reverses stock for every returned line in a
/// single transaction. ITransactionalRequest ties the pipeline into the
/// existing transactional behavior so a partial failure rolls back both
/// the SaleReturn rows and the StockMovement deltas.
/// </summary>
public record CreateSaleReturnCommand(CreateSaleReturnRequest Request)
    : IRequest<Guid>, ITransactionalRequest;
