namespace NextErp.Application.DTOs.UnitOfMeasure;

public sealed record UpdateUnitOfMeasureRequest
{
    public string Name { get; init; } = null!;
    public string Abbreviation { get; init; } = null!;
    public string? Category { get; init; }
    public bool IsActive { get; init; }
}
