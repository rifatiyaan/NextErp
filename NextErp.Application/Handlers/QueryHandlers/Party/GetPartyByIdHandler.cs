using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Queries;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Party
{
    public class GetPartyByIdHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetPartyByIdQuery, Entities.Party?>
    {
        public async Task<Entities.Party?> Handle(GetPartyByIdQuery request, CancellationToken cancellationToken)
        {
            return await unitOfWork.PartyRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        }
    }
}
