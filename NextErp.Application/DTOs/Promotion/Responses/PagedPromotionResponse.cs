namespace NextErp.Application.DTOs.Promotion;

public sealed record PagedPromotionResponse
{
    public int Total { get; init; }
    public int TotalDisplay { get; init; }
    public List<PromotionResponse> Data { get; init; } = new();
}
