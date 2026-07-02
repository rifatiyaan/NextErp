namespace NextErp.Application.DTOs.Sale;

public sealed record PreviewSaleRequest
{
    public List<PreviewLineRequest> Lines { get; init; } = new();
    public Guid? PartyId { get; init; }
}
