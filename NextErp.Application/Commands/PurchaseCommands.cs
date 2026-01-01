using MediatR;
using NextErp.Application.DTOs;

namespace NextErp.Application.Commands
{
    public record CreatePurchaseCommand(
        string Title,
        string PurchaseNumber,
        int SupplierId,
        DateTime PurchaseDate,
        List<Purchase.Request.Create.PurchaseItemRequest> Items,
        Purchase.Request.Metadata? Metadata
    ) : IRequest<Guid>; // Returns Id of created purchase
}
