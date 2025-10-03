using MediatR;

namespace EcommerceApplicationWeb.Application.Features.Products.Commands
{
    public class CreateProductCommand : IRequest<int>
    {
        public string Title { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int? ParentId { get; set; }
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Warranty { get; set; }

        public CreateProductCommand(
            string title,
            string code,
            int? parentId,
            int categoryId,
            decimal price,
            int stock,
            string? imageUrl = null,
            string? description = null,
            string? color = null,
            string? warranty = null)
        {
            Title = title;
            Code = code;
            ParentId = parentId;
            CategoryId = categoryId;
            Price = price;
            Stock = stock;
            ImageUrl = imageUrl;
            Description = description;
            Color = color;
            Warranty = warranty;
        }
    }
}
