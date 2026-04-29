using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Builders;

public class ProductBuilder
{
    private int _id;
    private string _title = "Test Product";
    private string _code = $"P-{Guid.NewGuid():N}".Substring(0, 8);
    private decimal _price = 100m;
    private int _categoryId = 1;
    private bool _isActive = true;
    private Guid _tenantId = Guid.NewGuid();
    private Guid _branchId = Guid.NewGuid();
    private DateTime _createdAt = DateTime.UtcNow;
    private bool _hasVariations;

    public ProductBuilder WithId(int id) { _id = id; return this; }
    public ProductBuilder WithTitle(string title) { _title = title; return this; }
    public ProductBuilder WithCode(string code) { _code = code; return this; }
    public ProductBuilder WithPrice(decimal price) { _price = price; return this; }
    public ProductBuilder WithCategory(int categoryId) { _categoryId = categoryId; return this; }
    public ProductBuilder WithTenant(Guid tenantId) { _tenantId = tenantId; return this; }
    public ProductBuilder WithBranch(Guid branchId) { _branchId = branchId; return this; }
    public ProductBuilder Inactive() { _isActive = false; return this; }
    public ProductBuilder CreatedAt(DateTime createdAt) { _createdAt = createdAt; return this; }
    public ProductBuilder WithVariations() { _hasVariations = true; return this; }

    public Product Build() => new()
    {
        Id = _id,
        Title = _title,
        Code = _code,
        Price = _price,
        CategoryId = _categoryId,
        IsActive = _isActive,
        TenantId = _tenantId,
        BranchId = _branchId,
        CreatedAt = _createdAt,
        HasVariations = _hasVariations,
    };
}
