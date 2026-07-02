namespace NextErp.Application.DTOs.ProductVariation;

public sealed record VariationOptionResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public List<VariationValueResponse> Values { get; set; } = new();
}
