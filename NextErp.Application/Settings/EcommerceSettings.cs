using NextErp.Application.Common.Settings;

namespace NextErp.Application.Settings;

// Store display currency. Each maps to an ISO 4217 code + a formatting locale
// in GetStoreConfigHandler; the storefront renders every price in this currency.
public enum StoreCurrency
{
    NOK,
    USD,
    EUR,
    GBP,
    BDT,
    INR,
    AED,
    SAR,
}

// Curated full storefront palettes. Each maps (in StoreThemeResolver) to a
// complete, coherent set of colours — background, surface, text, borders and
// accent — including a dark option. The whole storefront is token-driven, so
// switching this swaps the entire look.
public enum StorePalette
{
    Light,
    Slate,
    WarmSand,
    Midnight,
    Forest,
    Rose,
    Ocean,
    Graphite,
    Berry,
}

[SettingsModule("Ecommerce", "Ecommerce / Storefront")]
public sealed class EcommerceSettings
{
    [Setting(
        description: "Master switch. Off = every public store endpoint returns 403 and the storefront shows a closed page.",
        displayName: "Storefront enabled")]
    public bool StorefrontEnabled { get; set; } = false;

    [Setting(description: "Public store name shown in the header and page titles.", displayName: "Store name")]
    public string StoreName { get; set; } = "NextErp Store";

    [Setting(description: "Short tagline under the store name (optional).", displayName: "Tagline")]
    public string Tagline { get; set; } = "";

    [Setting(description: "Homepage hero headline.", displayName: "Hero headline")]
    public string HeroHeadline { get; set; } = "Objects, honestly made.";

    [Setting(description: "Homepage hero image URL (optional).", displayName: "Hero image URL")]
    public string HeroImageUrl { get; set; } = "";

    [Setting(description: "Marquee ribbon text on the homepage.", displayName: "Marquee text")]
    public string MarqueeText { get; set; } = "Cash on delivery — no account needed";

    [Setting(description: "Short cash-on-delivery explanation shown at checkout.", displayName: "COD note")]
    public string CodNote { get; set; } = "Pay in cash when your order arrives.";

    [Setting(description: "Flat delivery fee added to every online order.", displayName: "Delivery fee")]
    [SettingRange(0, 100000)]
    public decimal DeliveryFee { get; set; } = 0m;

    [Setting(description: "Currency all storefront prices are shown in.", displayName: "Store currency")]
    public StoreCurrency Currency { get; set; } = StoreCurrency.NOK;

    [Setting(
        description: "Storefront colour palette — a full coherent theme (background, surface, text, accent), including dark options. Choose from the curated set.",
        displayName: "Store palette")]
    public StorePalette Palette { get; set; } = StorePalette.Light;

    [Setting(
        description: "Advanced: sell from one specific branch (below). Off = the store auto-uses your default branch, so a single-branch shop needs no setup.",
        displayName: "Enable branch selling")]
    public bool EnableBranchSelling { get; set; } = false;

    [Setting(
        description: "Only used when 'Enable branch selling' is on: the branch whose stock and orders the storefront uses. Leave on Auto to use the default branch.",
        displayName: "Selling branch")]
    [SettingOptions("branches")]
    public string SellingBranchId { get; set; } = "";

    // Home hero carousel slides. Managed by the ecommerce settings banner
    // manager (image upload + reorder), not the generic settings grid, so this
    // is storage-only (no [Setting]). Holds a JSON array of
    // { imageUrl, headline?, subtext?, href? }.
    public string HeroSlidesJson { get; set; } = "";

    // Homepage section layout (the storefront "template engine" config). Managed
    // by the ecommerce settings homepage builder — storage-only (no [Setting]).
    // Holds a JSON array of { id, type, enabled, ...settings }; empty = the
    // storefront's built-in default layout.
    public string HomeLayoutJson { get; set; } = "";
}
