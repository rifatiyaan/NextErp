namespace NextErp.Application.DTOs.ProductVariation;

public sealed record ProductVariantRequest
{
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal InitialStock { get; set; }
    public bool IsActive { get; set; } = true;
    // Format: "optionIndex:valueIndex" - e.g., ["0:0", "1:1"] means first option's first value + second option's second value
    public List<string> VariationValueKeys { get; set; } = new();
}
