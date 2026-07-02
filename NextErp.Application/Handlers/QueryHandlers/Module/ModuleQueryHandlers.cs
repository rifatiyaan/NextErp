using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using NextErp.Application.Mapping;
using NextErp.Application.Queries.Module;
using NextErp.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NextErp.Application.Common.Caching;

namespace NextErp.Application.Handlers.QueryHandlers.Module
{
    public class GetMenuByUserHandler(
        IApplicationDbContext dbContext,
        IMemoryCache cache,
        IModuleCacheSignal cacheSignal)
        : IRequestHandler<GetMenuByUserQuery, List<DTOs.Module.ModuleResponse>>
    {
        public async Task<List<DTOs.Module.ModuleResponse>> Handle(GetMenuByUserQuery request, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"module:menu:{request.TenantId}";
            if (cache.TryGetValue(cacheKey, out List<DTOs.Module.ModuleResponse>? cached) && cached is not null)
                return cached;

            var menuItems = await dbContext.Modules
                .AsNoTracking()
                .Where(x => x.TenantId == request.TenantId && x.IsActive)
                .OrderBy(x => x.Order)
                .ToListAsync(cancellationToken);

            var tree = BuildMenuTree(menuItems.Select(m => m.ToResponse()).ToList());

            cache.Set(cacheKey, tree, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            }.AddExpirationToken(cacheSignal.Token));

            return tree;
        }

        private List<DTOs.Module.ModuleResponse> BuildMenuTree(List<DTOs.Module.ModuleResponse> flatList)
        {
            var lookup = flatList.ToDictionary(x => x.Id);
            var rootItems = new List<DTOs.Module.ModuleResponse>();

            foreach (var item in flatList)
            {
                if (item.ParentId.HasValue && lookup.TryGetValue(item.ParentId.Value, out var parent))
                {
                    parent.Children ??= new List<DTOs.Module.ModuleResponse>();
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

    public class GetAllModulesHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetAllModulesQuery, List<DTOs.Module.ModuleResponse>>
    {
        public async Task<List<DTOs.Module.ModuleResponse>> Handle(GetAllModulesQuery request, CancellationToken cancellationToken = default)
        {
            var items = await dbContext.Modules
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return items.Select(i => i.ToResponse()).ToList();
        }
    }

    public class GetModuleByIdHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetModuleByIdQuery, DTOs.Module.ModuleResponse?>
    {
        public async Task<DTOs.Module.ModuleResponse?> Handle(GetModuleByIdQuery request, CancellationToken cancellationToken = default)
        {
            var item = await dbContext.Modules
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            return item?.ToResponse();
        }
    }

    public class GetModulesByTypeHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetModulesByTypeQuery, List<DTOs.Module.ModuleResponse>>
    {
        public async Task<List<DTOs.Module.ModuleResponse>> Handle(GetModulesByTypeQuery request, CancellationToken cancellationToken = default)
        {
            var items = await dbContext.Modules
                .AsNoTracking()
                .Where(x => x.Type == (ModuleType)request.Type)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return items.Select(i => i.ToResponse()).ToList();
        }
    }
}
