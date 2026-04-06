using MediatR;
using NextErp.Application.Common.Attributes;
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
    ) : IRequest<Guid>; // Returns Id of created sale
}
