using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Common.Settings;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Services;

// TenantId currently hardcoded to Guid.Empty per the single-tenant pet-mode
// convention; swap to an injected resolver when proper multi-tenancy lands.
public sealed class SettingsProvider : ISettingsProvider
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    private readonly IApplicationDbContext _db;
    private readonly IReadOnlyDictionary<Type, ModuleRegistration> _byType;
    private readonly IReadOnlyDictionary<string, ModuleRegistration> _byName;
    private readonly ConcurrentDictionary<(Guid, string), string> _cache = new();

    public SettingsProvider(IApplicationDbContext db)
    {
        _db = db;
        var registrations = DiscoverModules();
        _byType = registrations.ToDictionary(r => r.Type);
        _byName = registrations.ToDictionary(r => r.ModuleName, StringComparer.OrdinalIgnoreCase);
    }

    private static List<ModuleRegistration> DiscoverModules()
    {
        // Scan the Application assembly where [SettingsModule] classes live.
        // Looking at one known type pins the assembly without a hard config.
        var assembly = typeof(ISettingsProvider).Assembly;
        return assembly.GetExportedTypes()
            .Where(t => t.GetCustomAttribute<SettingsModuleAttribute>() != null)
            .Select(BuildRegistration)
            .ToList();
    }

    private static ModuleRegistration BuildRegistration(Type type)
    {
        var moduleAttr = type.GetCustomAttribute<SettingsModuleAttribute>()!;
        var defaultInstance = Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"Settings type {type.FullName} must have a parameterless constructor.");

        var properties = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<SettingAttribute>() != null && p.CanRead && p.CanWrite)
            .ToList();

        var defs = new List<SettingDefinition>(properties.Count);
        foreach (var p in properties)
        {
            var attr = p.GetCustomAttribute<SettingAttribute>()!;
            var range = p.GetCustomAttribute<SettingRangeAttribute>();
            var (kind, options) = ClassifyType(p.PropertyType);
            defs.Add(new SettingDefinition
            {
                // camelCase so the schema key lines up with the API's
                // DictionaryKeyPolicy=CamelCase output for the values dict.
                Key = ToCamelCase(p.Name),
                DisplayName = attr.DisplayName ?? Humanize(p.Name),
                Description = attr.Description,
                Type = kind,
                Options = options,
                Min = range?.Min,
                Max = range?.Max,
                Default = p.GetValue(defaultInstance),
            });
        }

        return new ModuleRegistration(
            Type: type,
            ModuleName: moduleAttr.Name,
            DisplayName: moduleAttr.DisplayName,
            Properties: properties,
            Definitions: defs);
    }

    private static string ToCamelCase(string pascal) =>
        string.IsNullOrEmpty(pascal) || char.IsLower(pascal[0])
            ? pascal
            : char.ToLowerInvariant(pascal[0]) + pascal[1..];

    private static (string Kind, List<string>? Options) ClassifyType(Type t)
    {
        var underlying = Nullable.GetUnderlyingType(t) ?? t;
        if (underlying == typeof(bool)) return ("bool", null);
        if (underlying == typeof(int) || underlying == typeof(long)) return ("int", null);
        if (underlying == typeof(decimal) || underlying == typeof(double) || underlying == typeof(float)) return ("decimal", null);
        if (underlying == typeof(string)) return ("string", null);
        if (underlying.IsEnum) return ("enum", Enum.GetNames(underlying).ToList());
        throw new InvalidOperationException($"Unsupported setting type: {t.FullName}");
    }

    private static string Humanize(string pascal)
    {
        // "EnablePricingPreview" → "Enable pricing preview"
        if (string.IsNullOrEmpty(pascal)) return pascal;
        var sb = new System.Text.StringBuilder(pascal.Length + 4);
        sb.Append(pascal[0]);
        for (var i = 1; i < pascal.Length; i++)
        {
            var c = pascal[i];
            if (char.IsUpper(c) && !char.IsUpper(pascal[i - 1])) sb.Append(' ').Append(char.ToLowerInvariant(c));
            else sb.Append(c);
        }
        return sb.ToString();
    }

    private static Guid GetTenantId() => Guid.Empty;

    public SettingsSchema GetSchema() => new()
    {
        Modules = _byName.Values
            .OrderBy(r => r.ModuleName, StringComparer.OrdinalIgnoreCase)
            .Select(r => new SettingsModuleSchema
            {
                // Match the DictionaryKeyPolicy on the values endpoint.
                Name = ToCamelCase(r.ModuleName),
                DisplayName = r.DisplayName,
                Settings = r.Definitions.ToList(),
            })
            .ToList(),
    };

    public async Task<T> GetAsync<T>(CancellationToken cancellationToken = default) where T : class, new()
    {
        if (!_byType.TryGetValue(typeof(T), out var reg))
            throw new InvalidOperationException($"Type {typeof(T).FullName} is not a registered [SettingsModule].");

        var json = await LoadJsonAsync(reg.ModuleName, cancellationToken);
        return (T)Deserialize(reg, json);
    }

    public async Task<T> UpdateAsync<T>(T settings, CancellationToken cancellationToken = default) where T : class, new()
    {
        if (!_byType.TryGetValue(typeof(T), out var reg))
            throw new InvalidOperationException($"Type {typeof(T).FullName} is not a registered [SettingsModule].");

        var json = JsonSerializer.Serialize(settings, settings.GetType(), JsonOpts);
        await SaveJsonAsync(reg.ModuleName, json, cancellationToken);
        return (T)Deserialize(reg, json);
    }

    public async Task PatchAsync(string moduleName, IReadOnlyDictionary<string, object?> values, CancellationToken cancellationToken = default)
    {
        if (!_byName.TryGetValue(moduleName, out var reg))
            throw new InvalidOperationException($"Unknown settings module '{moduleName}'.");

        // Hydrate current (default-merged), apply incoming patches with type
        // coercion, validate ranges, then serialise back.
        var current = Deserialize(reg, await LoadJsonAsync(reg.ModuleName, cancellationToken));
        foreach (var (key, rawValue) in values)
        {
            var prop = reg.Properties.FirstOrDefault(p => string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Unknown setting '{key}' in module '{moduleName}'.");

            var coerced = CoerceValue(prop.PropertyType, rawValue, key, moduleName);
            ApplyRangeCheck(reg, prop, coerced);
            prop.SetValue(current, coerced);
        }

        var json = JsonSerializer.Serialize(current, reg.Type, JsonOpts);
        await SaveJsonAsync(reg.ModuleName, json, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, object?>>> GetAllValuesAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();

        // One round-trip for every module's blob (was N queries — one per
        // module via LoadJsonAsync). Warm the cache so later single-module
        // reads hit memory.
        var byModule = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var rows = await _db.ModuleSettings
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .Select(s => new { s.Module, s.SettingsJson })
            .ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            byModule[row.Module] = row.SettingsJson;
            _cache[(tenantId, row.Module)] = row.SettingsJson;
        }

        var result = new Dictionary<string, IReadOnlyDictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);
        foreach (var reg in _byName.Values)
        {
            byModule.TryGetValue(reg.ModuleName, out var json);
            var instance = Deserialize(reg, json);
            var moduleValues = new Dictionary<string, object?>(reg.Properties.Count);
            foreach (var p in reg.Properties)
            {
                moduleValues[ToCamelCase(p.Name)] = p.GetValue(instance);
            }
            result[ToCamelCase(reg.ModuleName)] = moduleValues;
        }
        return result;
    }

    private async Task<string?> LoadJsonAsync(string module, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        if (_cache.TryGetValue((tenantId, module), out var cached)) return cached;

        var row = await _db.ModuleSettings
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.Module == module)
            .Select(s => s.SettingsJson)
            .FirstOrDefaultAsync(ct);

        if (row != null) _cache[(tenantId, module)] = row;
        return row;
    }

    private async Task SaveJsonAsync(string module, string json, CancellationToken ct)
    {
        var tenantId = GetTenantId();
        var row = await _db.ModuleSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Module == module, ct);
        if (row == null)
        {
            _db.ModuleSettings.Add(new ModuleSetting
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Module = module,
                SettingsJson = json,
                CreatedAt = DateTime.UtcNow,
            });
        }
        else
        {
            row.SettingsJson = json;
            row.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct);
        _cache[(tenantId, module)] = json;
    }

    private static object Deserialize(ModuleRegistration reg, string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Activator.CreateInstance(reg.Type)!;
        var instance = JsonSerializer.Deserialize(json, reg.Type, JsonOpts);
        return instance ?? Activator.CreateInstance(reg.Type)!;
    }

    private static object? CoerceValue(Type targetType, object? raw, string key, string module)
    {
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (raw == null) return null;

        try
        {
            // System.Text.Json hands us JsonElement for incoming raw values.
            if (raw is JsonElement je)
            {
                return je.ValueKind switch
                {
                    JsonValueKind.True or JsonValueKind.False when underlying == typeof(bool) => je.GetBoolean(),
                    JsonValueKind.Number when underlying == typeof(int) => je.GetInt32(),
                    JsonValueKind.Number when underlying == typeof(long) => je.GetInt64(),
                    JsonValueKind.Number when underlying == typeof(decimal) => je.GetDecimal(),
                    JsonValueKind.Number when underlying == typeof(double) => je.GetDouble(),
                    JsonValueKind.String when underlying == typeof(string) => je.GetString(),
                    JsonValueKind.String when underlying.IsEnum => Enum.Parse(underlying, je.GetString()!, ignoreCase: true),
                    _ => throw new InvalidOperationException($"Cannot coerce {je.ValueKind} to {underlying.Name} for '{module}.{key}'."),
                };
            }
            if (underlying.IsEnum && raw is string s) return Enum.Parse(underlying, s, ignoreCase: true);
            return Convert.ChangeType(raw, underlying);
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or ArgumentException)
        {
            throw new InvalidOperationException($"Invalid value for '{module}.{key}': {raw} cannot be coerced to {underlying.Name}.");
        }
    }

    private static void ApplyRangeCheck(ModuleRegistration reg, PropertyInfo prop, object? value)
    {
        var range = prop.GetCustomAttribute<SettingRangeAttribute>();
        if (range == null || value == null) return;
        var asDouble = Convert.ToDouble(value);
        if (asDouble < range.Min || asDouble > range.Max)
            throw new InvalidOperationException(
                $"Value for '{reg.ModuleName}.{prop.Name}' must be between {range.Min} and {range.Max}; got {asDouble}.");
    }

    private sealed record ModuleRegistration(
        Type Type,
        string ModuleName,
        string DisplayName,
        IReadOnlyList<PropertyInfo> Properties,
        IReadOnlyList<SettingDefinition> Definitions);
}
