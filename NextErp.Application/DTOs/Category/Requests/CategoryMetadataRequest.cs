namespace NextErp.Application.DTOs.Category;

public sealed record CategoryMetadataRequest
{
    public string? ProductCount { get; set; }
    public string? Department { get; set; }
}
