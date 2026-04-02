using MediatR;
using NextErp.Application.Queries;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Party
{
    public class GetPagedPartiesHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetPagedPartiesQuery, (IList<Entities.Party> Records, int Total, int TotalDisplay)>
    {
        public async Task<(IList<Entities.Party> Records, int Total, int TotalDisplay)> Handle(
            GetPagedPartiesQuery request,
            CancellationToken cancellationToken)
        {
            return await unitOfWork.PartyRepository.GetTableDataAsync(
                request.PageIndex,
                request.PageSize,
                request.SearchText,
                request.SortBy,
                request.PartyType);
        }
    }
}
