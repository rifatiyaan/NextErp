using NextErp.Application.DTOs;
using MediatR;
using NextErp.Application.Common.Interfaces;

namespace NextErp.Application.Commands.Module
{
    public record CreateModuleCommand(DTOs.Module.Request.Create.Single Dto) : IRequest<int>, ITransactionalRequest;

    public record CreateBulkModulesCommand(DTOs.Module.Request.Create.Bulk Dto) : IRequest<DTOs.Module.Response.Create.Bulk>, ITransactionalRequest;

    public record UpdateModuleCommand(int Id, DTOs.Module.Request.Update.Single Dto) : IRequest<Unit>, ITransactionalRequest;

    public record DeleteModuleCommand(int Id) : IRequest<Unit>, ITransactionalRequest;
}
