using MediatR;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Party
{
    public class CreatePartyHandler(
        IApplicationUnitOfWork unitOfWork,
        IBranchProvider branchProvider)
        : IRequestHandler<CreatePartyCommand, Guid>
    {
        public async Task<Guid> Handle(CreatePartyCommand request, CancellationToken cancellationToken)
        {
            var party = new Entities.Party
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                ContactPerson = request.ContactPerson,
                LoyaltyCode = request.LoyaltyCode,
                NationalId = request.NationalId,
                VatNumber = request.VatNumber,
                TaxId = request.TaxId,
                Notes = request.Notes,
                PartyType = request.PartyType,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                BranchId = branchProvider.GetRequiredBranchId()
            };

            await unitOfWork.PartyRepository.AddAsync(party);
            await unitOfWork.SaveAsync();
            return party.Id;
        }
    }
}
