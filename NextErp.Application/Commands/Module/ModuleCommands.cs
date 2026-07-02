using NextErp.Application.DTOs;
using MediatR;
using NextErp.Application.Common.Interfaces;

namespace NextErp.Application.Commands.Module
{
    public record CreateModuleCommand(DTOs.Module.CreateModuleRequest Dto) : IRequest<int>, ITransactionalRequest;

    public record CreateBulkModulesCommand(DTOs.Module.CreateBulkModulesRequest Dto) : IRequest<DTOs.Module.CreateBulkModulesResponse>, ITransactionalRequest;

    public record UpdateModuleCommand(int Id, DTOs.Module.UpdateModuleRequest Dto) : IRequest<Unit>, ITransactionalRequest;

    public record DeleteModuleCommand(int Id) : IRequest<Unit>, ITransactionalRequest;
}
