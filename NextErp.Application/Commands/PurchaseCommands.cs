using MediatR;
using NextErp.Application.Common.Interfaces;
using NextErp.Application.DTOs;

namespace NextErp.Application.Commands
{
    public record CreatePurchaseCommand(
        string Title,
        string PurchaseNumber,
        Guid? PartyId,
        DateTime PurchaseDate,
        decimal Discount,
        List<Purchase.Request.Create.PurchaseItemRequest> Items,
        Purchase.Request.Metadata? Metadata
    ) : IRequest<Guid>, ITransactionalRequest; // Returns Id of created purchase
}
