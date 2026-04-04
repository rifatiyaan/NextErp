namespace NextErp.Domain.Common;

/// <summary>
/// Marks an entity as scoped to a branch. The persistence layer applies a global query filter
/// when the current user is not in global (SuperAdmin) scope.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class BranchScopedAttribute : Attribute
{
}
