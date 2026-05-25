namespace NextErp.Domain.Entities;

// Absence of a row for (TenantId, Module) means "use defaults" — read path
// merges via SettingsProvider; only operator-set keys are persisted.
public class ModuleSetting : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "Module Setting";

    public Guid TenantId { get; set; }

    public string Module { get; set; } = null!;

    public string SettingsJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
