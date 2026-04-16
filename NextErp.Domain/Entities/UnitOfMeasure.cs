namespace NextErp.Domain.Entities;

public class UnitOfMeasure : IEntity<int>
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Abbreviation { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
