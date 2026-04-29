using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Builders;

public class BranchBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _tenantId = Guid.NewGuid();
    private string _title = "Test Branch";
    private bool _isActive = true;

    public BranchBuilder WithId(Guid id) { _id = id; return this; }
    public BranchBuilder WithTenant(Guid tenantId) { _tenantId = tenantId; return this; }
    public BranchBuilder WithTitle(string title) { _title = title; return this; }
    public BranchBuilder Inactive() { _isActive = false; return this; }

    public Branch Build() => new()
    {
        Id = _id,
        TenantId = _tenantId,
        Title = _title,
        IsActive = _isActive,
        CreatedAt = DateTime.UtcNow,
    };
}
