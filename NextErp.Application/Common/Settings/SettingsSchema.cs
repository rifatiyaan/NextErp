namespace NextErp.Application.Common.Settings;

public sealed class SettingsSchema
{
    public List<SettingsModuleSchema> Modules { get; init; } = new();
}

public sealed class SettingsModuleSchema
{
    public string Name { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
    public List<SettingDefinition> Settings { get; init; } = new();
}

public sealed class SettingDefinition
{
    public string Key { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
    public string? Description { get; init; }

    // One of: "bool", "int", "decimal", "string", "enum".
    public string Type { get; init; } = null!;

    public List<string>? Options { get; init; }

    public double? Min { get; init; }
    public double? Max { get; init; }

    public object? Default { get; init; }
}
