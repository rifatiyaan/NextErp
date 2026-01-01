using MediatR;
using NextErp.Application.DTOs;

namespace NextErp.Application.Commands
{
    public record CreateSaleCommand(
        string Title,
        string SaleNumber,
        Guid CustomerId,
        DateTime SaleDate,
        List<Sale.Request.Create.SaleItemRequest> Items,
        Sale.Request.Metadata? Metadata
    ) : IRequest<Guid>; // Returns Id of created sale
}
