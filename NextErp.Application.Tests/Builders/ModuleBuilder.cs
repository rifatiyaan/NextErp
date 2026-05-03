using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Builders;

public class ModuleBuilder
{
    private int _id;
    private string _title = "Test Module";
    private string? _icon;
    private string? _url;
    private int? _parentId;
    private ModuleType _type = ModuleType.Link;
    private int _order;
    private bool _isActive = true;
    private Guid _tenantId = Guid.Empty;

    public ModuleBuilder WithId(int id) { _id = id; return this; }
    public ModuleBuilder WithTitle(string title) { _title = title; return this; }
    public ModuleBuilder WithIcon(string? icon) { _icon = icon; return this; }
    public ModuleBuilder WithUrl(string? url) { _url = url; return this; }
    public ModuleBuilder WithParent(int? parentId) { _parentId = parentId; return this; }
    public ModuleBuilder WithType(ModuleType type) { _type = type; return this; }
    public ModuleBuilder WithOrder(int order) { _order = order; return this; }
    public ModuleBuilder Inactive() { _isActive = false; return this; }
    public ModuleBuilder WithTenant(Guid tenantId) { _tenantId = tenantId; return this; }

    public Module Build() => new()
    {
        Id = _id,
        Title = _title,
        Icon = _icon,
        Url = _url,
        ParentId = _parentId,
        Type = _type,
        Order = _order,
        IsActive = _isActive,
        TenantId = _tenantId,
        CreatedAt = DateTime.UtcNow,
    };
}
