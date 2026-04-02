using MediatR;
using NextErp.Application.DTOs;

namespace NextErp.Application.Commands
{
    public record CreateSaleCommand(
        Guid? PartyId,
        decimal Discount,
        string? PaymentMethod,
        decimal? PaidAmount,
        List<Sale.Request.Create.SaleItemRequest> Items
    ) : IRequest<Guid>; // Returns Id of created sale
}
