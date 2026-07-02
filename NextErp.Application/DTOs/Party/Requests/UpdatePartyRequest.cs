namespace NextErp.Application.DTOs.Party;

public sealed record UpdatePartyRequest : PartyRequestBase
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;
}
