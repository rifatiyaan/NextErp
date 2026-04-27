using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Party
{
    public class GetPartyByIdHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetPartyByIdQuery, Entities.Party?>
    {
        public async Task<Entities.Party?> Handle(GetPartyByIdQuery request, CancellationToken cancellationToken = default)
        {
            return await dbContext.Parties
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        }
    }
}
