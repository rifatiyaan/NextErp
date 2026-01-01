using NextErp.Application.Queries;
using MediatR;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Customer
{
    public class GetPagedCustomersHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetPagedCustomersQuery, (IList<Entities.Customer> Records, int Total, int TotalDisplay)>
    {
        public async Task<(IList<Entities.Customer> Records, int Total, int TotalDisplay)> Handle(
            GetPagedCustomersQuery request,
            CancellationToken cancellationToken)
        {
            return await unitOfWork.CustomerRepository.GetTableDataAsync(
                request.PageIndex,
                request.PageSize,
                request.SearchText,
                request.SortBy);
        }
    }
}
