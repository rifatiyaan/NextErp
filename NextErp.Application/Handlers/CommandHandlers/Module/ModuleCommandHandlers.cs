using AutoMapper;
using NextErp.Application.Commands.Module;
using NextErp.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Handlers.CommandHandlers.Module
{
    public class CreateModuleHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<CreateModuleCommand, int>
    {
        public async Task<int> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
        {
            var module = mapper.Map<NextErp.Domain.Entities.Module>(request.Dto);
            module.CreatedAt = DateTime.UtcNow;

            await unitOfWork.ModuleRepository.AddAsync(module);
            await unitOfWork.SaveAsync();

            return module.Id;
        }
    }

    public class CreateBulkModulesHandler(
        IApplicationUnitOfWork unitOfWork,
        IApplicationDbContext dbContext,
        IMapper mapper)
        : IRequestHandler<CreateBulkModulesCommand, DTOs.Module.Response.Create.Bulk>
    {
        public async Task<DTOs.Module.Response.Create.Bulk> Handle(
            CreateBulkModulesCommand request,
            CancellationToken cancellationToken)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted,
                cancellationToken);

            var response = new DTOs.Module.Response.Create.Bulk
            {
                Modules = new List<DTOs.Module.Response.Create.Hierarchical>(),
                SuccessCount = 0,
                FailureCount = 0,
                Errors = new List<string>()
            };

            try
            {
                var parentModules = new List<(DTOs.Module.Request.Create.Hierarchical Dto, NextErp.Domain.Entities.Module Entity)>();

                foreach (var moduleDto in request.Dto.Modules)
                {
                    try
                    {
                        var module = mapper.Map<NextErp.Domain.Entities.Module>(moduleDto);
                        module.ParentId = null;
                        module.CreatedAt = DateTime.UtcNow;
                        module.TenantId = Guid.Empty;

                        await unitOfWork.ModuleRepository.AddAsync(module);
                        parentModules.Add((moduleDto, module));
                    }
                    catch (Exception ex)
                    {
                        response.FailureCount++;
                        response.Errors.Add($"Failed to create parent module '{moduleDto.Title}': {ex.Message}");
                    }
                }

                await unitOfWork.SaveAsync();

                var allChildren = new List<(DTOs.Module.Request.Create.Hierarchical ChildDto, int ParentId)>();

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
                        var childModule = mapper.Map<NextErp.Domain.Entities.Module>(childDto);
                        childModule.ParentId = parentId;
                        childModule.CreatedAt = DateTime.UtcNow;
                        childModule.TenantId = Guid.Empty;

                        await unitOfWork.ModuleRepository.AddAsync(childModule);
                    }
                    catch (Exception ex)
                    {
                        response.FailureCount++;
                        response.Errors.Add($"Failed to create child module '{childDto.Title}': {ex.Message}");
                    }
                }

                await unitOfWork.SaveAsync();
                await transaction.CommitAsync(cancellationToken);

                var childErrorCount = response.Errors.Count(e => e.Contains("child module"));
                response.SuccessCount = parentModules.Count + (allChildren.Count - childErrorCount);

                foreach (var (dto, entity) in parentModules)
                {
                    var responseModule = mapper.Map<DTOs.Module.Response.Create.Hierarchical>(entity);
                    response.Modules.Add(responseModule);
                }

                return response;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public class UpdateModuleHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<UpdateModuleCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
        {
            var existing = await unitOfWork.ModuleRepository.GetByIdAsync(request.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Module with ID {request.Id} not found.");

            mapper.Map(request.Dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.ModuleRepository.EditAsync(existing);
            await unitOfWork.SaveAsync();

            return Unit.Value;
        }
    }

    public class DeleteModuleHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<DeleteModuleCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
        {
            var existing = await unitOfWork.ModuleRepository.GetByIdAsync(request.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Module with ID {request.Id} not found.");

            await unitOfWork.ModuleRepository.RemoveAsync(existing);
            await unitOfWork.SaveAsync();

            return Unit.Value;
        }
    }
}
