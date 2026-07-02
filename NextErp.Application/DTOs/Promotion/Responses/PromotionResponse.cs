using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs.Promotion;

public sealed record PromotionResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public PromotionType Type { get; init; }
    public string TypeName => Type.ToString();
    public PromotionConfigDto Config { get; init; } = new();
    public bool IsActive { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int Priority { get; init; }
    public bool Stackable { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
