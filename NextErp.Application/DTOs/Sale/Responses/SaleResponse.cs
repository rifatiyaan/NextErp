using NextErp.Application.DTOs.Payment;

namespace NextErp.Application.DTOs.Sale;

public sealed record SaleResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string SaleNumber { get; init; } = null!;
    public Guid? PartyId { get; init; }
    public string CustomerName { get; init; } = null!;
    public DateTime SaleDate { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal Discount { get; init; }
    public decimal Tax { get; init; }
    public decimal FinalAmount { get; init; }
    public decimal TotalPaid { get; init; }
    public decimal BalanceDue { get; init; }
    public List<SaleItemResponse> Items { get; init; } = new();
    public List<PaymentLineResponse> Payments { get; init; } = new();
    public SaleMetadataRequest Metadata { get; init; } = new();
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid? BranchId { get; init; }
}
