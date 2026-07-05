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

    // One of: "bool", "int", "decimal", "string", "enum", "select".
    public string Type { get; init; } = null!;

    // Static option list for "enum" (value == label).
    public List<string>? Options { get; init; }

    // Named dynamic-options source for "select" (e.g. "branches"); the schema
    // handler resolves it into Choices before the schema is returned.
    public string? OptionsSource { get; init; }

    // Resolved value/label pairs for a "select" (populated by the schema handler).
    public List<SettingOption>? Choices { get; set; }

    public double? Min { get; init; }
    public double? Max { get; init; }

    public object? Default { get; init; }
}

public sealed class SettingOption
{
    public string Value { get; init; } = null!;
    public string Label { get; init; } = null!;
}
