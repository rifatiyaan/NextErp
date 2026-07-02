using Microsoft.AspNetCore.Http;

namespace NextErp.Application.DTOs.Product;

public sealed record ProductImageSlotRequest
{
    public string? Url { get; set; }
    public IFormFile? File { get; set; }
    public bool IsThumbnail { get; set; }
}
