using AutoMapper;
using NextErp.Application.DTOs;
using NextErp.Application.Queries.Module;
using NextErp.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Application.Handlers.QueryHandlers.Module
{
    public class GetMenuByUserHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper) 
        : IRequestHandler<GetMenuByUserQuery, List<DTOs.Module.Response.Get.Single>>
    {
        public async Task<List<DTOs.Module.Response.Get.Single>> Handle(GetMenuByUserQuery request, CancellationToken cancellationToken)
        {
            // Get all modules for the tenant and filter by active status
            var menuItems = await unitOfWork.ModuleRepository.Query()
                .Where(x => x.TenantId == request.TenantId && x.IsActive)
                .ToListAsync(cancellationToken);
            
            var dtos = mapper.Map<List<DTOs.Module.Response.Get.Single>>(menuItems);

            // Build tree structure from flat list
            return BuildMenuTree(dtos);
        }

        private List<DTOs.Module.Response.Get.Single> BuildMenuTree(List<DTOs.Module.Response.Get.Single> flatList)
        {
            var lookup = flatList.ToDictionary(x => x.Id);
            var rootItems = new List<DTOs.Module.Response.Get.Single>();

            foreach (var item in flatList)
            {
                if (item.ParentId.HasValue && lookup.TryGetValue(item.ParentId.Value, out var parent))
                {
                    parent.Children ??= new List<DTOs.Module.Response.Get.Single>();
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

    public class GetAllModulesHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper) 
        : IRequestHandler<GetAllModulesQuery, List<DTOs.Module.Response.Get.Single>>
    {
        public async Task<List<DTOs.Module.Response.Get.Single>> Handle(GetAllModulesQuery request, CancellationToken cancellationToken)
        {
            var items = await unitOfWork.ModuleRepository.Query()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return mapper.Map<List<DTOs.Module.Response.Get.Single>>(items);
        }
    }

    public class GetModuleByIdHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper) 
        : IRequestHandler<GetModuleByIdQuery, DTOs.Module.Response.Get.Single?>
    {
        public async Task<DTOs.Module.Response.Get.Single?> Handle(GetModuleByIdQuery request, CancellationToken cancellationToken)
        {
            var item = await unitOfWork.ModuleRepository.GetByIdAsync(request.Id);
            return mapper.Map<DTOs.Module.Response.Get.Single?>(item);
        }
    }

    public class GetModulesByTypeHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetModulesByTypeQuery, List<DTOs.Module.Response.Get.Single>>
    {
        public async Task<List<DTOs.Module.Response.Get.Single>> Handle(GetModulesByTypeQuery request, CancellationToken cancellationToken)
        {
            var items = await unitOfWork.ModuleRepository.Query()
                .Where(x => x.Type == (ModuleType)request.Type)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return mapper.Map<List<DTOs.Module.Response.Get.Single>>(items);
        }
    }
}
