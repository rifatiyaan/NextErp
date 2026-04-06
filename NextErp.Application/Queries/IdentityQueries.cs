using MediatR;
using NextErp.Application.DTOs;

namespace NextErp.Application.Queries
{
    public record GetIdentityDashboardQuery : IRequest<IdentityCommandCenterDto>;
}
