namespace NextErp.Application.DTOs.Dashboard;

public sealed record DashboardRevenuePointResponse
{
    /// <summary>"Jan", "Feb", … (3-letter month label).</summary>
    public string Month { get; init; } = null!;

    /// <summary>YYYY-MM string for unambiguous ordering on the client.</summary>
    public string YearMonth { get; init; } = null!;

    public decimal Revenue { get; init; }
    public int Orders { get; init; }
}
