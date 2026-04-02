using MediatR;
using NextErp.Domain.Entities;

namespace NextErp.Application.Commands
{
    public record CreatePartyCommand(
        string Title,
        string? FirstName,
        string? LastName,
        string? Email,
        string? Phone,
        string? Address,
        string? ContactPerson,
        string? LoyaltyCode,
        string? NationalId,
        string? VatNumber,
        string? TaxId,
        string? Notes,
        PartyType PartyType,
        bool IsActive = true
    ) : IRequest<Guid>;

    public record UpdatePartyCommand(
        Guid Id,
        string Title,
        string? FirstName,
        string? LastName,
        string? Email,
        string? Phone,
        string? Address,
        string? ContactPerson,
        string? LoyaltyCode,
        string? NationalId,
        string? VatNumber,
        string? TaxId,
        string? Notes,
        PartyType PartyType,
        bool IsActive = true
    ) : IRequest<Unit>;
}
