namespace NextErp.Application.DTOs.ProductVariation;

public sealed record ProductVariantResponse
{
    public int Id { get; set; }
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal AvailableQuantity { get; set; }
    public bool IsActive { get; set; }
    public string Title { get; set; } = null!; // e.g., "S / Red"
    public List<VariationValueResponse> VariationValues { get; set; } = new();
}
