using MediatR;
using NextErp.Application.DTOs;

namespace NextErp.Application.Queries
{
    /// <summary>
    /// Returns all roles, users, and branches in a single round-trip
    /// for the Identity Command Center frontend.
    /// </summary>
    public record GetIdentityDashboardQuery : IRequest<IdentityCommandCenterDto>;
}
