using NextErp.Application.DTOs;
using MediatR;

namespace NextErp.Application.Commands.Module
{
    public record CreateModuleCommand(ModuleRequestDto Dto) : IRequest<int>;

    public record UpdateModuleCommand(int Id, ModuleRequestDto Dto) : IRequest<Unit>;

    public record DeleteModuleCommand(int Id) : IRequest<Unit>;
}
