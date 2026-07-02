namespace NextErp.Application.DTOs.Sale;

public sealed record SaleReportRequest
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public Guid? PartyId { get; init; }
}
