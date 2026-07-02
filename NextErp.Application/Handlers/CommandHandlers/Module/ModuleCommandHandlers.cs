using NextErp.Application.Commands.Module;
using NextErp.Application.DTOs;
using NextErp.Application.Mapping;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Common.Caching;
using DomainModule = NextErp.Domain.Entities.Module;

namespace NextErp.Application.Handlers.CommandHandlers.Module
{
    public class CreateModuleHandler(IApplicationDbContext dbContext, IModuleCacheSignal? cacheSignal = null)
        : IRequestHandler<CreateModuleCommand, int>
    {
        public async Task<int> Handle(CreateModuleCommand request, CancellationToken cancellationToken = default)
        {
            var module = request.Dto.ToEntity();
            module.CreatedAt = DateTime.UtcNow;

            dbContext.Modules.Add(module);
            await dbContext.SaveChangesAsync(cancellationToken);
            cacheSignal?.Invalidate();

            return module.Id;
        }
    }

    public class CreateBulkModulesHandler(IApplicationDbContext dbContext, IModuleCacheSignal? cacheSignal = null)
        : IRequestHandler<CreateBulkModulesCommand, DTOs.Module.CreateBulkModulesResponse>
    {
        public async Task<DTOs.Module.CreateBulkModulesResponse> Handle(
            CreateBulkModulesCommand request,
            CancellationToken cancellationToken = default)
        {
            var response = new DTOs.Module.CreateBulkModulesResponse
            {
                Modules = new List<DTOs.Module.CreateModuleHierarchicalResponse>(),
                SuccessCount = 0,
                FailureCount = 0,
                Errors = new List<string>()
            };

            var parentModules = new List<(DTOs.Module.CreateModuleHierarchicalRequest Dto, DomainModule Entity)>();

            foreach (var moduleDto in request.Dto.Modules)
            {
                try
                {
                    var module = moduleDto.ToEntity();
                    module.ParentId = null;
                    module.CreatedAt = DateTime.UtcNow;
                    module.TenantId = Guid.Empty;

                    dbContext.Modules.Add(module);
                    parentModules.Add((moduleDto, module));
                }
                catch (Exception ex)
                {
                    response.FailureCount++;
                    response.Errors.Add($"Failed to create parent module '{moduleDto.Title}': {ex.Message}");
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            var allChildren = new List<(DTOs.Module.CreateModuleHierarchicalRequest ChildDto, int ParentId)>();

            foreach (var (dto, entity) in parentModules)
            {
                if (dto.Children != null && dto.Children.Any())
                {
                    foreach (var childDto in dto.Children)
                    {
                        allChildren.Add((childDto, entity.Id));
                    }
                }
            }

            foreach (var (childDto, parentId) in allChildren)
            {
                try
                {
                    var childModule = childDto.ToEntity();
                    childModule.ParentId = parentId;
                    childModule.CreatedAt = DateTime.UtcNow;
                    childModule.TenantId = Guid.Empty;

                    dbContext.Modules.Add(childModule);
                }
                catch (Exception ex)
                {
                    response.FailureCount++;
                    response.Errors.Add($"Failed to create child module '{childDto.Title}': {ex.Message}");
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            cacheSignal?.Invalidate();

            var childErrorCount = response.Errors.Count(e => e.Contains("child module"));
            response.SuccessCount = parentModules.Count + (allChildren.Count - childErrorCount);

            foreach (var (dto, entity) in parentModules)
            {
                var responseModule = entity.ToCreateHierarchicalResponse();
                response.Modules.Add(responseModule);
            }

            return response;
        }
    }

    public class UpdateModuleHandler(IApplicationDbContext dbContext, IModuleCacheSignal? cacheSignal = null)
        : IRequestHandler<UpdateModuleCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateModuleCommand request, CancellationToken cancellationToken = default)
        {
            var existing = await dbContext.Modules
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (existing == null)
                throw new KeyNotFoundException($"Module with ID {request.Id} not found.");

            request.Dto.ApplyTo(existing);
            existing.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);
            cacheSignal?.Invalidate();

            return Unit.Value;
        }
    }

    public class DeleteModuleHandler(IApplicationDbContext dbContext, IModuleCacheSignal? cacheSignal = null)
        : IRequestHandler<DeleteModuleCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteModuleCommand request, CancellationToken cancellationToken = default)
        {
            var existing = await dbContext.Modules
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (existing == null)
                throw new KeyNotFoundException($"Module with ID {request.Id} not found.");

            // Module is ISoftDeletable — preserve original repo behaviour (soft-delete via IsActive flag).
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            cacheSignal?.Invalidate();

            return Unit.Value;
        }
    }
}
