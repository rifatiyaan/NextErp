namespace NextErp.Application.DTOs.Returns;

public sealed record SaleReturnResponse
{
    public Guid Id { get; init; }
    public string ReturnNumber { get; init; } = null!;
    public Guid SaleId { get; init; }
    public string SaleNumber { get; init; } = null!;
    public string? CustomerName { get; init; }
    public DateTime ReturnDate { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
    public decimal TotalRefund { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<SaleReturnLineResponse> Items { get; init; } = new();
}
