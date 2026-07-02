namespace NextErp.Application.DTOs.Sale;

public sealed record CreateSaleResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string SaleNumber { get; init; } = null!;
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}
