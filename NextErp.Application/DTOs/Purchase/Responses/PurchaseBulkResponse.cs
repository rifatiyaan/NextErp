namespace NextErp.Application.DTOs.Purchase;

public sealed record PurchaseBulkResponse
{
    public List<PurchaseResponse> Purchases { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}
