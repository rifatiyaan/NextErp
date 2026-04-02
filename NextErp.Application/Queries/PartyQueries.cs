using MediatR;
using NextErp.Domain.Entities;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Queries
{
    public record GetPartyByIdQuery(Guid Id) : IRequest<Entities.Party?>;

    public record GetPagedPartiesQuery(
        int PageIndex,
        int PageSize,
        string? SearchText,
        string? SortBy,
        PartyType? PartyType = null
    ) : IRequest<(IList<Entities.Party> Records, int Total, int TotalDisplay)>;
}
