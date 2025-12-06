using NextErp.Application.DTOs;
using MediatR;

namespace NextErp.Application.Queries.Module
{
    public record GetMenuByUserQuery(string[] Roles, Guid TenantId) : IRequest<List<ModuleResponseDto>>;

    public record GetAllModulesQuery(Guid TenantId) : IRequest<List<ModuleResponseDto>>;

    public record GetModuleByIdQuery(int Id) : IRequest<ModuleResponseDto?>;
    
    public record GetModulesByTypeQuery(int Type, Guid TenantId) : IRequest<List<ModuleResponseDto>>;
}
