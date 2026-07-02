namespace NextErp.Application.DTOs.Purchase;

public sealed record GetPurchaseReportRequest
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public Guid? PartyId { get; init; }
}
