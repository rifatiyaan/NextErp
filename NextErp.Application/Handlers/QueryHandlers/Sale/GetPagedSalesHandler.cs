using NextErp.Application.Queries;
using MediatR;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Sale
{
    public class GetPagedSalesHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetPagedSalesQuery, (IList<Entities.Sale> Records, int Total, int TotalDisplay)>
    {
        public async Task<(IList<Entities.Sale> Records, int Total, int TotalDisplay)> Handle(
            GetPagedSalesQuery request,
            CancellationToken cancellationToken)
        {
            return await unitOfWork.SaleRepository.GetTableDataAsync(
                request.PageIndex,
                request.PageSize,
                request.SearchText,
                request.SortBy);
        }
    }
}
