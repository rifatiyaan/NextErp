using MediatR;
using NextErp.Application.DTOs;

namespace NextErp.Application.Commands
{
    public record CreateSaleCommand(
        Guid? CustomerId,
        decimal TotalAmount,
        decimal Discount,
        decimal Tax,
        decimal FinalAmount,
        string? PaymentMethod,
        List<Sale.Request.Create.SaleItemRequest> Items
    ) : IRequest<Guid>; // Returns Id of created sale
}
