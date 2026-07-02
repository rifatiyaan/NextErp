namespace NextErp.Application.DTOs.Returns;

public sealed record CreateSaleReturnRequest
{
    public Guid SaleId { get; init; }
    public DateTime? ReturnDate { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
    public List<SaleReturnLineRequest> Items { get; init; } = new();
}
