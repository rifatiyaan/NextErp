using AutoMapper;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.Module;
using NextErp.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Application.Handlers.QueryHandlers.Module
{
    public class GetMenuByUserHandler(IApplicationDbContext dbContext, IMapper mapper)
        : IRequestHandler<GetMenuByUserQuery, List<DTOs.Module.Response.Get.Single>>
    {
        public async Task<List<DTOs.Module.Response.Get.Single>> Handle(GetMenuByUserQuery request, CancellationToken cancellationToken = default)
        {
            var menuItems = await dbContext.Modules
                .AsNoTracking()
                .Where(x => x.TenantId == request.TenantId && x.IsActive)
                .OrderBy(x => x.Order)
                .ToListAsync(cancellationToken);

            var dtos = mapper.Map<List<DTOs.Module.Response.Get.Single>>(menuItems);
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

    public class GetAllModulesHandler(IApplicationDbContext dbContext, IMapper mapper)
        : IRequestHandler<GetAllModulesQuery, List<DTOs.Module.Response.Get.Single>>
    {
        public async Task<List<DTOs.Module.Response.Get.Single>> Handle(GetAllModulesQuery request, CancellationToken cancellationToken = default)
        {
            var items = await dbContext.Modules
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return mapper.Map<List<DTOs.Module.Response.Get.Single>>(items);
        }
    }

    public class GetModuleByIdHandler(IApplicationDbContext dbContext, IMapper mapper)
        : IRequestHandler<GetModuleByIdQuery, DTOs.Module.Response.Get.Single?>
    {
        public async Task<DTOs.Module.Response.Get.Single?> Handle(GetModuleByIdQuery request, CancellationToken cancellationToken = default)
        {
            var item = await dbContext.Modules
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            return mapper.Map<DTOs.Module.Response.Get.Single?>(item);
        }
    }

    public class GetModulesByTypeHandler(IApplicationDbContext dbContext, IMapper mapper)
        : IRequestHandler<GetModulesByTypeQuery, List<DTOs.Module.Response.Get.Single>>
    {
        public async Task<List<DTOs.Module.Response.Get.Single>> Handle(GetModulesByTypeQuery request, CancellationToken cancellationToken = default)
        {
            var items = await dbContext.Modules
                .AsNoTracking()
                .Where(x => x.Type == (ModuleType)request.Type)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return mapper.Map<List<DTOs.Module.Response.Get.Single>>(items);
        }
    }
}
