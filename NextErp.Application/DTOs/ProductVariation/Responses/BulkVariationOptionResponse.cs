namespace NextErp.Application.DTOs.ProductVariation;

public sealed record BulkVariationOptionResponse
{
    public string Name { get; set; } = null!;
    public List<string> Values { get; set; } = new();
}
