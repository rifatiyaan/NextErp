using MediatR;
using NextErp.Application.Common.Interfaces;
using NextErp.Application.DTOs.Returns;

namespace NextErp.Application.Commands.PurchaseReturn;

/// <summary>
/// Creates a Purchase Return + decrements stock for every returned line in
/// a single transaction. Like its Sale-side sibling, this hooks into the
/// transactional pipeline so a failure rolls back both the PurchaseReturn
/// rows and the negative StockMovement deltas.
/// </summary>
public record CreatePurchaseReturnCommand(PurchaseReturnDto.Request.Create.Single Request)
    : IRequest<Guid>, ITransactionalRequest;
