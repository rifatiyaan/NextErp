namespace NextErp.Application.Common.Settings;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SettingAttribute(string? description = null, string? displayName = null) : Attribute
{
    public string? Description { get; } = description;
    public string? DisplayName { get; } = displayName;
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SettingRangeAttribute(double min, double max) : Attribute
{
    public double Min { get; } = min;
    public double Max { get; } = max;
}

// Marks a string setting as a dropdown whose options are resolved dynamically
// at schema time from a named source (e.g. "branches"). The stored value is the
// option's value; the label is display-only. See GetFeatureSettingsSchemaHandler.
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SettingOptionsAttribute(string sourceKey) : Attribute
{
    public string SourceKey { get; } = sourceKey;
}
