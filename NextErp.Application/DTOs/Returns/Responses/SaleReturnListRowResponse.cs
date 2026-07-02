namespace NextErp.Application.DTOs.Returns;

public sealed record SaleReturnListRowResponse
{
    public Guid Id { get; init; }
    public string ReturnNumber { get; init; } = null!;
    public Guid SaleId { get; init; }
    public string SaleNumber { get; init; } = null!;
    public string? CustomerName { get; init; }
    public DateTime ReturnDate { get; init; }
    public decimal TotalRefund { get; init; }
    public int ItemCount { get; init; }
}
