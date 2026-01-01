using NextErp.Application.Queries;
using MediatR;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Purchase
{
    public class GetPagedPurchasesHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetPagedPurchasesQuery, (IList<Entities.Purchase> Records, int Total, int TotalDisplay)>
    {
        public async Task<(IList<Entities.Purchase> Records, int Total, int TotalDisplay)> Handle(
            GetPagedPurchasesQuery request,
            CancellationToken cancellationToken)
        {
            return await unitOfWork.PurchaseRepository.GetTableDataAsync(
                request.PageIndex,
                request.PageSize,
                request.SearchText,
                request.SortBy);
        }
    }
}
