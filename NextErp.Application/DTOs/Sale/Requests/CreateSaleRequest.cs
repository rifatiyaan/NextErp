namespace NextErp.Application.DTOs.Sale;

public sealed record CreateSaleRequest
{
    public Guid? PartyId { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal Discount { get; init; } = 0;
    public decimal Tax { get; init; } = 0;
    public decimal FinalAmount { get; init; }
    public string? PaymentMethod { get; init; }
    public decimal? PaidAmount { get; init; }
    public List<SaleItemRequest> Items { get; init; } = new();
}
