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
}
