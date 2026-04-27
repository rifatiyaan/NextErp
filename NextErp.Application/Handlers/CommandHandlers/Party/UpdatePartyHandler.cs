using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Handlers.CommandHandlers.Party
{
    public class UpdatePartyHandler(IApplicationDbContext dbContext)
        : IRequestHandler<UpdatePartyCommand, Unit>
    {
        public async Task<Unit> Handle(UpdatePartyCommand request, CancellationToken cancellationToken = default)
        {
            var party = await dbContext.Parties
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (party == null)
                throw new InvalidOperationException($"Party with Id {request.Id} not found.");

            party.Title = request.Title;
            party.FirstName = request.FirstName;
            party.LastName = request.LastName;
            party.Email = request.Email;
            party.Phone = request.Phone;
            party.Address = request.Address;
            party.ContactPerson = request.ContactPerson;
            party.LoyaltyCode = request.LoyaltyCode;
            party.NationalId = request.NationalId;
            party.VatNumber = request.VatNumber;
            party.TaxId = request.TaxId;
            party.Notes = request.Notes;
            party.PartyType = request.PartyType;
            party.IsActive = request.IsActive;
            party.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
