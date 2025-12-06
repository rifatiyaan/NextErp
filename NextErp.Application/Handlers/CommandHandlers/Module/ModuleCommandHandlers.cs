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
}
