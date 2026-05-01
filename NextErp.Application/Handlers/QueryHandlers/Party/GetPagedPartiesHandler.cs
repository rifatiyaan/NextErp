using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Common.Extensions;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using System.Linq.Dynamic.Core;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Party
{
    public class GetPagedPartiesHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetPagedPartiesQuery, (IList<Entities.Party> Records, int Total, int TotalDisplay)>
    {
        public async Task<(IList<Entities.Party> Records, int Total, int TotalDisplay)> Handle(
            GetPagedPartiesQuery request,
            CancellationToken cancellationToken = default)
        {
            // Inlined from former IPartyRepository.GetTableDataAsync.
            var searchText = request.SearchText;

            var query = dbContext.Parties
                .AsNoTracking()
                .WhereIfHasValue(request.PartyType, x => x.PartyType == request.PartyType!.Value)
                .WhereIfNotNullOrEmpty(searchText, x =>
                    x.Title.Contains(searchText!) ||
                    (x.Email != null && x.Email.Contains(searchText!)) ||
                    (x.Phone != null && x.Phone.Contains(searchText!)));

            var total = await query.CountAsync(cancellationToken);

            // Preserve original dynamic-string sort behaviour via System.Linq.Dynamic.Core.
            var ordered = string.IsNullOrWhiteSpace(request.SortBy)
                ? (IQueryable<Entities.Party>)query
                : query.OrderBy(request.SortBy);

            var records = await ordered
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return (records, total, total);
        }
    }
}
