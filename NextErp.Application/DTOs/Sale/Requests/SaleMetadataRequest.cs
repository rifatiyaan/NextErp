namespace NextErp.Application.DTOs.Sale;

public sealed record SaleMetadataRequest
{
    public string? ReferenceNo { get; init; }
    public string? PaymentMethod { get; init; }
    public string? Notes { get; init; }
}
