using EcommerceApplicationWeb.Application.DTOs;
using MediatR;

namespace EcommerceApplicationWeb.Application.Commands.Menu
{
    public record CreateMenuItemCommand(MenuItemRequestDto Dto) : IRequest<int>;

    public record UpdateMenuItemCommand(int Id, MenuItemRequestDto Dto) : IRequest<Unit>;

    public record DeleteMenuItemCommand(int Id) : IRequest<Unit>;
}
