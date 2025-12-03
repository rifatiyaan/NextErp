using EcommerceApplicationWeb.Application.DTOs;
using MediatR;

namespace EcommerceApplicationWeb.Application.Queries.Menu
{
    public record GetMenuByUserQuery(string[] Roles, Guid TenantId) : IRequest<List<MenuItemResponseDto>>;

    public record GetAllMenuItemsQuery(Guid TenantId) : IRequest<List<MenuItemResponseDto>>;

    public record GetMenuItemByIdQuery(int Id) : IRequest<MenuItemResponseDto?>;
}
