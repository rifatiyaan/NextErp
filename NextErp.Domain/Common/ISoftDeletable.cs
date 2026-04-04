namespace NextErp.Domain.Common;

/// <summary>
/// Marks an entity that supports soft delete via <see cref="IsActive"/> instead of physical removal.
/// </summary>
public interface ISoftDeletable
{
    bool IsActive { get; set; }
}
