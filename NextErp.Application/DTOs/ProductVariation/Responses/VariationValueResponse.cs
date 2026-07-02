namespace NextErp.Application.DTOs.ProductVariation;

public sealed record VariationValueResponse
{
    public int Id { get; set; }
    public string Value { get; set; } = null!;
    public int DisplayOrder { get; set; }
}
