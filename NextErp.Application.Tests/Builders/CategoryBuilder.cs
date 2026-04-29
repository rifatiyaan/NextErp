using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Builders;

public class CategoryBuilder
{
    private int _id;
    private string _title = "Test Category";
    private Guid _tenantId = Guid.NewGuid();
    private Guid? _branchId;

    public CategoryBuilder WithId(int id) { _id = id; return this; }
    public CategoryBuilder WithTitle(string title) { _title = title; return this; }
    public CategoryBuilder WithTenant(Guid tenantId) { _tenantId = tenantId; return this; }
    public CategoryBuilder WithBranch(Guid? branchId) { _branchId = branchId; return this; }

    public Category Build() => new()
    {
        Id = _id,
        Title = _title,
        IsActive = true,
        TenantId = _tenantId,
        BranchId = _branchId,
        CreatedAt = DateTime.UtcNow,
    };
}
