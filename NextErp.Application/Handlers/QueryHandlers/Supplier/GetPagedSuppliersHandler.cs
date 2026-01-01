using NextErp.Application.Queries;
using MediatR;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Supplier
{
    public class GetPagedSuppliersHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetPagedSuppliersQuery, (IList<Entities.Supplier> Records, int Total, int TotalDisplay)>
    {
        public async Task<(IList<Entities.Supplier> Records, int Total, int TotalDisplay)> Handle(
            GetPagedSuppliersQuery request,
            CancellationToken cancellationToken)
        {
            return await unitOfWork.SupplierRepository.GetTableDataAsync(
                request.PageIndex,
                request.PageSize,
                request.SearchText,
                request.SortBy);
        }
    }
}
