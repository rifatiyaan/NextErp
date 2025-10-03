using MediatR;

namespace EcommerceApplicationWeb.Application.Features.Products.Commands
{
    public class SoftDeleteProductCommand : IRequest<Unit>
    {
        public int Id { get; set; }

        public SoftDeleteProductCommand(int id)
        {
            Id = id;
        }
    }
}
