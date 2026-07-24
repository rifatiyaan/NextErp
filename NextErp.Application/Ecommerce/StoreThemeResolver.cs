using NextErp.Application.DTOs.Ecommerce;
using NextErp.Application.Settings;

namespace NextErp.Application.Ecommerce;

/// <summary>
/// Maps the store's chosen palette to its full, curated colour set (background,
/// surface, subtle, ink, ink-soft, line, accent). Palette-only by design — the
/// storefront offers a fixed set of coherent themes, not free-form colours, so
/// every result is guaranteed readable and on-brand. All values are static
/// #rrggbb literals, so nothing user-entered ever reaches the client's styles.
/// </summary>
public static class StoreThemeResolver
{
    public static StoreThemeColors Resolve(StorePalette palette) => palette switch
    {
        StorePalette.Slate => new("#f8fafc", "#ffffff", "#eef2f7", "#1e293b", "#64748b", "#e2e8f0", "#0ea5e9"),
        StorePalette.WarmSand => new("#faf7f2", "#fffdf9", "#f3ece1", "#2a2420", "#7c6f5f", "#e8ddcd", "#c2410c"),
        StorePalette.Midnight => new("#0b1120", "#141b2d", "#1c2438", "#eef2f9", "#94a3b8", "#263149", "#3b82f6"),
        StorePalette.Forest => new("#f5f8f5", "#ffffff", "#e9f0ea", "#14261c", "#5c6b60", "#dce6dd", "#059669"),
        StorePalette.Rose => new("#fdf6f7", "#ffffff", "#f8e9ed", "#2b1a20", "#7d6068", "#f0dde2", "#e11d48"),
        StorePalette.Ocean => new("#f2fafd", "#ffffff", "#e3f2f7", "#0c2a33", "#5c7a83", "#d3e6ec", "#0891b2"),
        StorePalette.Graphite => new("#17181c", "#1f2126", "#282b31", "#f3f4f6", "#9ca3af", "#33363d", "#a78bfa"),
        StorePalette.Berry => new("#fdf4fb", "#ffffff", "#f8e6f4", "#2a1526", "#7a5f74", "#eed6ea", "#c026d3"),
        _ => new("#fafafa", "#ffffff", "#f5f6f8", "#0f172a", "#64748b", "#e7e9ee", "#2563eb"), // Light
    };
}
