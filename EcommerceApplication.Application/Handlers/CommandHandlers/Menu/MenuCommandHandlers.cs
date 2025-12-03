using AutoMapper;
using EcommerceApplicationWeb.Application.Commands.Menu;
using EcommerceApplicationWeb.Domain.Entities;
using MediatR;

namespace EcommerceApplicationWeb.Application.Handlers.CommandHandlers.Menu
{
    public class CreateMenuItemHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper) 
        : IRequestHandler<CreateMenuItemCommand, int>
    {
        public async Task<int> Handle(CreateMenuItemCommand request, CancellationToken cancellationToken)
        {
            var menuItem = mapper.Map<MenuItem>(request.Dto);
            
            // Set default tenant (would normally come from current user context)
            // For now, we assume it's handled or set to a default
            menuItem.TenantId = Guid.Empty; // Replace with actual tenant logic
            menuItem.CreatedAt = DateTime.UtcNow;

            await unitOfWork.MenuItemRepository.AddAsync(menuItem);
            await unitOfWork.SaveAsync();

            return menuItem.Id;
        }
    }

    public class UpdateMenuItemHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper) 
        : IRequestHandler<UpdateMenuItemCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateMenuItemCommand request, CancellationToken cancellationToken)
        {
            var menuItem = await unitOfWork.MenuItemRepository.GetByIdAsync(request.Id);
            
            if (menuItem == null)
                throw new KeyNotFoundException($"MenuItem with ID {request.Id} not found.");

            mapper.Map(request.Dto, menuItem);
            menuItem.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.MenuItemRepository.EditAsync(menuItem);
            await unitOfWork.SaveAsync();

            return Unit.Value;
        }
    }

    public class DeleteMenuItemHandler(IApplicationUnitOfWork unitOfWork) 
        : IRequestHandler<DeleteMenuItemCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteMenuItemCommand request, CancellationToken cancellationToken)
        {
            var menuItem = await unitOfWork.MenuItemRepository.GetByIdAsync(request.Id);
            
            if (menuItem == null)
                throw new KeyNotFoundException($"MenuItem with ID {request.Id} not found.");

            // Soft delete
            menuItem.IsActive = false;
            menuItem.UpdatedAt = DateTime.UtcNow;
            
            await unitOfWork.MenuItemRepository.EditAsync(menuItem);
            await unitOfWork.SaveAsync();

            return Unit.Value;
        }
    }
}
