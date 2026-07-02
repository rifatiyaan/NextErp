namespace NextErp.Application.DTOs.UnitOfMeasure;

public sealed record UnitOfMeasureResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Abbreviation { get; init; } = null!;
    public string? Category { get; init; }
    public bool IsSystem { get; init; }
    public bool IsActive { get; init; }
}
