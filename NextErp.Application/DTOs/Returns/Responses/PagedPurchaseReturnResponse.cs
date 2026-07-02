namespace NextErp.Application.DTOs.Returns;

public sealed record PagedPurchaseReturnResponse
{
    public int Total { get; init; }
    public int TotalDisplay { get; init; }
    public List<PurchaseReturnListRowResponse> Data { get; init; } = new();
}
