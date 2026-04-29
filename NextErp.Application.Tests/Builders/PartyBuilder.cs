using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Builders;

public class PartyBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _title = "Test Customer";
    private PartyType _partyType = PartyType.Customer;
    private Guid _tenantId = Guid.NewGuid();
    private Guid _branchId = Guid.NewGuid();

    public PartyBuilder WithId(Guid id) { _id = id; return this; }
    public PartyBuilder WithTitle(string title) { _title = title; return this; }
    public PartyBuilder AsSupplier() { _partyType = PartyType.Supplier; return this; }
    public PartyBuilder WithTenant(Guid tenantId) { _tenantId = tenantId; return this; }
    public PartyBuilder WithBranch(Guid branchId) { _branchId = branchId; return this; }

    public Party Build() => new()
    {
        Id = _id,
        Title = _title,
        PartyType = _partyType,
        IsActive = true,
        TenantId = _tenantId,
        BranchId = _branchId,
        CreatedAt = DateTime.UtcNow,
    };
}
