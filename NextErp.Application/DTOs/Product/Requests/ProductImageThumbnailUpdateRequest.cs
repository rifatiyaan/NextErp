namespace NextErp.Application.DTOs.Product;

public sealed record ProductImageThumbnailUpdateRequest
{
    public int Id { get; set; }
    public bool IsThumbnail { get; set; }
}
