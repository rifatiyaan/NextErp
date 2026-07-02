namespace NextErp.Application.DTOs.Returns;

public sealed record SaleReturnLineRequest
{
    public Guid SaleItemId { get; init; }
    public int ProductVariantId { get; init; }
    public decimal Quantity { get; init; }
    public decimal? UnitPrice { get; init; }
    public string? ConditionNote { get; init; }
}
