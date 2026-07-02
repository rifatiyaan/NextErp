using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs.Promotion;

public sealed record CreatePromotionRequest
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public PromotionType Type { get; init; }
    public PromotionConfigDto Config { get; init; } = new();
    public bool IsActive { get; init; } = true;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int Priority { get; init; }
    public bool Stackable { get; init; } = true;
}
