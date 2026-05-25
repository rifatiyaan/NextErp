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
