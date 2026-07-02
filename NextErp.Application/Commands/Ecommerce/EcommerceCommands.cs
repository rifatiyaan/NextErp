using MediatR;
using NextErp.Application.Common.Attributes;
using NextErp.Application.Common.Interfaces;

namespace NextErp.Application.Commands.Ecommerce
{
    [RequiresPermission("Settings.System.Manage")]
    public record SetEcommercePublicationCommand(
        List<int> PublishCategoryIds,
        List<int> UnpublishCategoryIds,
        List<int> PublishProductIds,
        List<int> UnpublishProductIds
    ) : IRequest<Unit>, ITransactionalRequest;

    // Anonymous guest checkout — deliberately has NO [RequiresPermission].
    public record CreateOnlineOrderCommand(
        string CustomerName,
        string Phone,
        string Address,
        string? Note,
        List<NextErp.Application.DTOs.Ecommerce.StoreOrderItemRequest> Items
    ) : IRequest<string>, ITransactionalRequest;
}
