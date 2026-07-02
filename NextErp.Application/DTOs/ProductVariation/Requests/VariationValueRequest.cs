namespace NextErp.Application.DTOs.ProductVariation;

public sealed record VariationValueRequest
{
    public string Value { get; set; } = null!; // e.g., "S", "Red"
    public int DisplayOrder { get; set; } = 0;
}
