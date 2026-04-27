using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Category
{
    public class GetCategoryByIdHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetCategoryByIdQuery, Entities.Category?>
    {
        public async Task<Entities.Category?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken = default)
        {
            return await dbContext.Categories
                .AsNoTracking()
                .Include(c => c.Parent)
                .Include(c => c.Children)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.IsActive, cancellationToken);
        }
    }
}
