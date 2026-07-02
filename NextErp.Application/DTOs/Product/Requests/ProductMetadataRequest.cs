namespace NextErp.Application.DTOs.Product;

public sealed record ProductMetadataRequest
{
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Warranty { get; set; }
    public int? CategoryId { get; set; }
}
