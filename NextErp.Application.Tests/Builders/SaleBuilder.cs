using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Builders;

public class SaleBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _saleNumber = $"S-{Guid.NewGuid():N}".Substring(0, 10);
    private string _title = "Test Sale";
    private Guid? _partyId;
    private DateTime _saleDate = DateTime.UtcNow;
    private decimal _totalAmount = 100m;
    private decimal _finalAmount = 100m;
    private Guid _tenantId = Guid.NewGuid();
    private Guid _branchId = Guid.NewGuid();

    public SaleBuilder WithId(Guid id) { _id = id; return this; }
    public SaleBuilder WithSaleNumber(string saleNumber) { _saleNumber = saleNumber; return this; }
    public SaleBuilder WithTitle(string title) { _title = title; return this; }
    public SaleBuilder WithParty(Guid partyId) { _partyId = partyId; return this; }
    public SaleBuilder WithSaleDate(DateTime saleDate) { _saleDate = saleDate; return this; }
    public SaleBuilder WithTotalAmount(decimal totalAmount) { _totalAmount = totalAmount; return this; }
    public SaleBuilder WithFinalAmount(decimal finalAmount) { _finalAmount = finalAmount; return this; }
    public SaleBuilder WithTenant(Guid tenantId) { _tenantId = tenantId; return this; }
    public SaleBuilder WithBranch(Guid branchId) { _branchId = branchId; return this; }

    public Sale Build() => new()
    {
        Id = _id,
        SaleNumber = _saleNumber,
        Title = _title,
        PartyId = _partyId,
        SaleDate = _saleDate,
        TotalAmount = _totalAmount,
        Discount = 0m,
        Tax = 0m,
        FinalAmount = _finalAmount,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        TenantId = _tenantId,
        BranchId = _branchId,
        Items = new List<SaleItem>(),
        Payments = new List<SalePayment>(),
    };
}
