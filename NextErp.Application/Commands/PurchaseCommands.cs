using MediatR;
using NextErp.Application.Common.Interfaces;
using NextErp.Application.DTOs.Purchase;

namespace NextErp.Application.Commands
{
    public record CreatePurchaseCommand(
        string Title,
        string PurchaseNumber,
        Guid? PartyId,
        DateTime PurchaseDate,
        decimal Discount,
        List<PurchaseItemRequest> Items,
        PurchaseMetadataRequest? Metadata
    ) : IRequest<Guid>, ITransactionalRequest; // Returns Id of created purchase
}
