using MediatR;
using NextErp.Application.Common.Attributes;
using NextErp.Application.Common.Interfaces;
using NextErp.Application.DTOs;

namespace NextErp.Application.Commands
{
    [RequiresPermission("Sale.Create")]
    public record CreateSaleCommand(
        Guid? PartyId,
        decimal Discount,
        string? PaymentMethod,
        decimal? PaidAmount,
        List<Sale.Request.Create.SaleItemRequest> Items
    ) : IRequest<Guid>, ITransactionalRequest; // Returns Id of created sale
}
