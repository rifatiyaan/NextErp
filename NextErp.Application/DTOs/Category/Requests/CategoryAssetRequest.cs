namespace NextErp.Application.DTOs.Category;

public sealed record CategoryAssetRequest
{
    public string Filename { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string Type { get; set; } = "image";
    public long? Size { get; set; }
    public DateTime UploadedAt { get; set; }
}
