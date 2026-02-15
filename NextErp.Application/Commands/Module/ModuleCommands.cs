using NextErp.Application.DTOs;
using MediatR;

namespace NextErp.Application.Commands.Module
{
    public record CreateModuleCommand(DTOs.Module.Request.Create.Single Dto) : IRequest<int>;

    public record CreateBulkModulesCommand(DTOs.Module.Request.Create.Bulk Dto) : IRequest<DTOs.Module.Response.Create.Bulk>;

    public record UpdateModuleCommand(int Id, DTOs.Module.Request.Update.Single Dto) : IRequest<Unit>;

    public record DeleteModuleCommand(int Id) : IRequest<Unit>;
}
