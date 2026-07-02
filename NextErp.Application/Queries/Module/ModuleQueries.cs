using NextErp.Application.DTOs;
using MediatR;

namespace NextErp.Application.Queries.Module
{
    public record GetModuleByIdQuery(int Id) : IRequest<DTOs.Module.ModuleResponse?>;

    public record GetAllModulesQuery() : IRequest<List<DTOs.Module.ModuleResponse>>;

    public record GetModulesByTypeQuery(int Type) : IRequest<List<DTOs.Module.ModuleResponse>>;

    public record GetMenuByUserQuery(Guid UserId, Guid TenantId) : IRequest<List<DTOs.Module.ModuleResponse>>;
}
