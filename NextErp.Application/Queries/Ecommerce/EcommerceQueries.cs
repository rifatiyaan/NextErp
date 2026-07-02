using MediatR;
using NextErp.Application.DTOs.Ecommerce;

namespace NextErp.Application.Queries.Ecommerce
{
    public record GetEcommercePublicationQuery() : IRequest<List<PublicationCategoryResponse>>;
}
