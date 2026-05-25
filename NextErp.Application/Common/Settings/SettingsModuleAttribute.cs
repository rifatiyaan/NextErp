namespace NextErp.Application.Common.Settings;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class SettingsModuleAttribute(string name, string? displayName = null) : Attribute
{
    public string Name { get; } = name;
    public string DisplayName { get; } = displayName ?? name;
}
