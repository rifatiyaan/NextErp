using NextErp.Application.DTOs;
using MediatR;

namespace NextErp.Application.Queries.Module
{
    public record GetModuleByIdQuery(int Id) : IRequest<DTOs.Module.Response.Get.Single?>;

    public record GetAllModulesQuery() : IRequest<List<DTOs.Module.Response.Get.Single>>;

    public record GetModulesByTypeQuery(int Type) : IRequest<List<DTOs.Module.Response.Get.Single>>;

    public record GetMenuByUserQuery(Guid UserId, Guid TenantId) : IRequest<List<DTOs.Module.Response.Get.Single>>;
}
