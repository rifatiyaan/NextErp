namespace NextErp.Application.DTOs.Dashboard;

public sealed record DashboardActivityRowResponse
{
    /// <summary>"sale", "purchase", or "stock" — small enough that a string is fine.</summary>
    public string Kind { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string? Subtitle { get; init; }
    public decimal? Amount { get; init; }
    public DateTime OccurredAt { get; init; }
}
