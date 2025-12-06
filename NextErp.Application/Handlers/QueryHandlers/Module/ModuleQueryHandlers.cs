using AutoMapper;
using NextErp.Application.DTOs;
using NextErp.Application.Queries.Module;
using NextErp.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Application.Handlers.QueryHandlers.Module
{
    public class GetMenuByUserHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper) 
        : IRequestHandler<GetMenuByUserQuery, List<ModuleResponseDto>>
    {
        public async Task<List<ModuleResponseDto>> Handle(GetMenuByUserQuery request, CancellationToken cancellationToken)
        {
            var menuItems = await unitOfWork.ModuleRepository.GetMenuByUserRolesAsync(request.Roles, request.TenantId);
            
            var dtos = mapper.Map<List<ModuleResponseDto>>(menuItems);

            // Build tree structure from flat list
            return BuildMenuTree(dtos);
        }

        private List<ModuleResponseDto> BuildMenuTree(List<ModuleResponseDto> flatList)
        {
            var lookup = flatList.ToDictionary(x => x.Id);
            var rootItems = new List<ModuleResponseDto>();

            foreach (var item in flatList)
            {
                if (item.ParentId.HasValue && lookup.TryGetValue(item.ParentId.Value, out var parent))
                {
                    parent.Children ??= new List<ModuleResponseDto>();
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
        : IRequestHandler<GetAllModulesQuery, List<ModuleResponseDto>>
    {
        public async Task<List<ModuleResponseDto>> Handle(GetAllModulesQuery request, CancellationToken cancellationToken)
        {
            var items = await unitOfWork.ModuleRepository.Query()
                .Where(x => x.TenantId == request.TenantId)
                .OrderBy(x => x.Order)
                .ToListAsync(cancellationToken);

            return mapper.Map<List<ModuleResponseDto>>(items);
        }
    }

    public class GetModuleByIdHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper) 
        : IRequestHandler<GetModuleByIdQuery, ModuleResponseDto?>
    {
        public async Task<ModuleResponseDto?> Handle(GetModuleByIdQuery request, CancellationToken cancellationToken)
        {
            var item = await unitOfWork.ModuleRepository.GetByIdAsync(request.Id);
            return mapper.Map<ModuleResponseDto?>(item);
        }
    }

    public class GetModulesByTypeHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetModulesByTypeQuery, List<ModuleResponseDto>>
    {
        public async Task<List<ModuleResponseDto>> Handle(GetModulesByTypeQuery request, CancellationToken cancellationToken)
        {
            var items = await unitOfWork.ModuleRepository.Query()
                .Where(x => x.TenantId == request.TenantId && x.Type == (ModuleType)request.Type)
                .OrderBy(x => x.Order)
                .ToListAsync(cancellationToken);

            return mapper.Map<List<ModuleResponseDto>>(items);
        }
    }
}
