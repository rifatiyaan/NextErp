using AutoMapper;
using NextErp.Application.DTOs;
using NextErp.Application.Queries.Menu;
using NextErp.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Application.Handlers.QueryHandlers.Menu
{
    public class GetMenuByUserHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper) 
        : IRequestHandler<GetMenuByUserQuery, List<MenuItemResponseDto>>
    {
        public async Task<List<MenuItemResponseDto>> Handle(GetMenuByUserQuery request, CancellationToken cancellationToken)
        {
            // Get all active items for the tenant
            // We use the repository method but we might need to adjust it to ensure we get a flat list 
            // and then build the tree to avoid N+1 or deep recursion issues in EF
            
            // For simplicity, let's assume the repository returns a flat list of all allowed items
            var menuItems = await unitOfWork.MenuItemRepository.GetMenuByUserRolesAsync(request.Roles, request.TenantId);
            
            var dtos = mapper.Map<List<MenuItemResponseDto>>(menuItems);

            // Build tree structure from flat list
            return BuildMenuTree(dtos);
        }

        private List<MenuItemResponseDto> BuildMenuTree(List<MenuItemResponseDto> flatList)
        {
            var lookup = flatList.ToDictionary(x => x.Id);
            var rootItems = new List<MenuItemResponseDto>();

            foreach (var item in flatList)
            {
                if (item.ParentId.HasValue && lookup.TryGetValue(item.ParentId.Value, out var parent))
                {
                    parent.Children ??= new List<MenuItemResponseDto>();
                    parent.Children.Add(item);
                }
                else
                {
                    rootItems.Add(item);
                }
            }

            return rootItems.OrderBy(x => x.Order).ToList();
        }
    }

    public class GetAllMenuItemsHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper) 
        : IRequestHandler<GetAllMenuItemsQuery, List<MenuItemResponseDto>>
    {
        public async Task<List<MenuItemResponseDto>> Handle(GetAllMenuItemsQuery request, CancellationToken cancellationToken)
        {
            var items = await unitOfWork.MenuItemRepository.Query()
                .Where(x => x.TenantId == request.TenantId)
                .OrderBy(x => x.Order)
                .ToListAsync(cancellationToken);

            return mapper.Map<List<MenuItemResponseDto>>(items);
        }
    }

    public class GetMenuItemByIdHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper) 
        : IRequestHandler<GetMenuItemByIdQuery, MenuItemResponseDto?>
    {
        public async Task<MenuItemResponseDto?> Handle(GetMenuItemByIdQuery request, CancellationToken cancellationToken)
        {
            var item = await unitOfWork.MenuItemRepository.GetByIdAsync(request.Id);
            return mapper.Map<MenuItemResponseDto?>(item);
        }
    }
}
