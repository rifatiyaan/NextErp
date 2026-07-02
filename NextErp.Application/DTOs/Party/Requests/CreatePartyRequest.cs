namespace NextErp.Application.DTOs.Party;

public sealed record CreatePartyRequest : PartyRequestBase
{
    public bool IsActive { get; set; } = true;
}
