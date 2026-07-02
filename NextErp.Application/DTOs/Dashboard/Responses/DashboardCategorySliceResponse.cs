namespace NextErp.Application.DTOs.Dashboard;

public sealed record DashboardCategorySliceResponse
{
    public int? CategoryId { get; init; }
    public string CategoryName { get; init; } = null!;
    public decimal Revenue { get; init; }
    public int ItemCount { get; init; }
}
