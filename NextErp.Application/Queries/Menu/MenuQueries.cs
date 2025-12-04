using NextErp.Application.DTOs;
using MediatR;

namespace NextErp.Application.Queries.Menu
{
    public record GetMenuByUserQuery(string[] Roles, Guid TenantId) : IRequest<List<MenuItemResponseDto>>;

    public record GetAllMenuItemsQuery(Guid TenantId) : IRequest<List<MenuItemResponseDto>>;

    public record GetMenuItemByIdQuery(int Id) : IRequest<MenuItemResponseDto?>;
}
