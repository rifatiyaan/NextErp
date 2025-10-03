using EcommerceApplicationWeb.Domain.Entities;
using MediatR;

namespace EcommerceApplicationWeb.Application.Features.Products.Queries
{
    public class GetProductByIdQuery : IRequest<Product?>
    {
        public int Id { get; set; }

        public GetProductByIdQuery(int id)
        {
            Id = id;
        }
    }
}
