namespace NextErp.Application.DTOs.UnitOfMeasure;

public sealed record CreateUnitOfMeasureRequest
{
    public string Name { get; init; } = null!;
    public string Abbreviation { get; init; } = null!;
    public string? Category { get; init; }
    public bool IsSystem { get; init; } = false;
}
