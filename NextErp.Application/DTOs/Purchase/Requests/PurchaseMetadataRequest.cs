namespace NextErp.Application.DTOs.Purchase;

public sealed record PurchaseMetadataRequest
{
    public string? BatchNo { get; init; }
    public string? BillNo { get; init; }
    public string? ChallanNo { get; init; }
    public string? ReferenceNo { get; init; }
    public string? Notes { get; init; }
}
