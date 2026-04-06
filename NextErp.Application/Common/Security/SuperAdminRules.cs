namespace NextErp.Application.Common.Security;

public static class SuperAdminRules
{
    public const string SuperAdminRoleName = "SuperAdmin";

    public static Guid? SuperAdminRoleId { get; set; }

    public static bool IsSuperAdmin(string? primaryRoleName, Guid? primaryRoleId)
    {
        if (primaryRoleId.HasValue
            && SuperAdminRoleId.HasValue
            && primaryRoleId.Value == SuperAdminRoleId.Value)
            return true;

        if (string.IsNullOrEmpty(primaryRoleName))
            return false;

        return primaryRoleName.Equals(SuperAdminRoleName, StringComparison.OrdinalIgnoreCase)
            || primaryRoleName.Equals("Superadmin", StringComparison.OrdinalIgnoreCase);
    }
}
