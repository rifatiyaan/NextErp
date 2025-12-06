using AutoMapper;
using NextErp.Application.Commands.Module;
using NextErp.Domain.Entities;
using MediatR;

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

    public class BulkCreateModuleHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<BulkCreateModuleCommand, List<int>>
    {
        public async Task<List<int>> Handle(BulkCreateModuleCommand request, CancellationToken cancellationToken)
        {
            var createdIds = new List<int>();

            foreach (var moduleDto in request.Modules)
            {
                var id = await CreateModuleRecursive(moduleDto, null);
                createdIds.Add(id);
            }

            await unitOfWork.SaveAsync();
            return createdIds;
        }

        private async Task<int> CreateModuleRecursive(NextErp.Application.DTOs.BulkModuleDto dto, int? parentId)
        {
            var module = mapper.Map<NextErp.Domain.Entities.Module>(dto);
            module.ParentId = parentId;
            module.CreatedAt = DateTime.UtcNow;
            
            // Explicitly set metadata if present
            if (dto.Metadata != null)
            {
                module.Metadata = new NextErp.Domain.Entities.Module.ModuleMetadata
                {
                   Author = dto.Metadata.Author,
                   Website = dto.Metadata.Website,
                   Dependencies = dto.Metadata.Dependencies,
                   BadgeText = dto.Metadata.BadgeText,
                   BadgeColor = dto.Metadata.BadgeColor,
                   Roles = dto.Metadata.Roles,
                   Description = dto.Metadata.Description,
                   OpenInNewTab = dto.Metadata.OpenInNewTab,
                   ConfigurationUrl = dto.Metadata.ConfigurationUrl
                };
            }

            // We must save each level immediately to get the ID for children
            await unitOfWork.ModuleRepository.AddAsync(module);
            await unitOfWork.SaveAsync(); 

            foreach (var childDto in dto.Children)
            {
                await CreateModuleRecursive(childDto, module.Id);
            }

            return module.Id;
        }
    }
}
