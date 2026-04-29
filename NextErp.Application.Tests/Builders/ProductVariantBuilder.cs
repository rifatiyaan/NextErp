using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Builders;

public class ProductVariantBuilder
{
    private int _id;
    private string _title = "Default Variant";
    private string _sku = $"SKU-{Guid.NewGuid():N}".Substring(0, 12);
    private decimal _price = 100m;
    private int _productId = 1;
    private Guid _tenantId = Guid.NewGuid();
    private Guid? _branchId;

    public ProductVariantBuilder WithId(int id) { _id = id; return this; }
    public ProductVariantBuilder WithTitle(string title) { _title = title; return this; }
    public ProductVariantBuilder WithSku(string sku) { _sku = sku; return this; }
    public ProductVariantBuilder WithPrice(decimal price) { _price = price; return this; }
    public ProductVariantBuilder WithProduct(int productId) { _productId = productId; return this; }
    public ProductVariantBuilder WithTenant(Guid tenantId) { _tenantId = tenantId; return this; }
    public ProductVariantBuilder WithBranch(Guid? branchId) { _branchId = branchId; return this; }

    public ProductVariant Build() => new()
    {
        Id = _id,
        Title = _title,
        Name = _title,
        Sku = _sku,
        Price = _price,
        ProductId = _productId,
        TenantId = _tenantId,
        BranchId = _branchId,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
    };
}
