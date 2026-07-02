namespace NextErp.Application.DTOs.Returns;

public sealed record PagedSaleReturnResponse
{
    public int Total { get; init; }
    public int TotalDisplay { get; init; }
    public List<SaleReturnListRowResponse> Data { get; init; } = new();
}
