using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Builders;

public class UnitOfMeasureBuilder
{
    private int _id;
    private string _name = "Piece";
    private string _abbreviation = "pc";
    private string? _category = "Count";
    private bool _isSystem;
    private bool _isActive = true;

    public UnitOfMeasureBuilder WithId(int id) { _id = id; return this; }
    public UnitOfMeasureBuilder WithName(string name) { _name = name; return this; }
    public UnitOfMeasureBuilder WithAbbreviation(string abbreviation) { _abbreviation = abbreviation; return this; }
    public UnitOfMeasureBuilder WithCategory(string? category) { _category = category; return this; }
    public UnitOfMeasureBuilder AsSystem() { _isSystem = true; return this; }
    public UnitOfMeasureBuilder Inactive() { _isActive = false; return this; }

    public UnitOfMeasure Build() => new()
    {
        Id = _id,
        Title = _name,
        Name = _name,
        Abbreviation = _abbreviation,
        Category = _category,
        IsSystem = _isSystem,
        IsActive = _isActive,
        CreatedAt = DateTime.UtcNow,
    };
}
