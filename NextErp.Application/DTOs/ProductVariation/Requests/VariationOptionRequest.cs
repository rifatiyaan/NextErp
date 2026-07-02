namespace NextErp.Application.DTOs.ProductVariation;

public sealed record VariationOptionRequest
{
    public string Name { get; set; } = null!; // e.g., "Size", "Color"
    public int DisplayOrder { get; set; } = 0;
    public List<VariationValueRequest> Values { get; set; } = new(); // e.g., ["S", "M", "L"] or ["Red", "Blue"]
}
