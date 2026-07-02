namespace NextErp.Application.DTOs.Product;

public sealed record ProductImageItemResponse
{
    public int Id { get; set; }
    public string Url { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public bool IsThumbnail { get; set; }
}
