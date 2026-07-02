using NextErp.Application.Common.Settings;

namespace NextErp.Application.Settings;

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

    [Setting(description: "Branch whose stock and orders the storefront uses.", displayName: "Selling branch id")]
    public string SellingBranchId { get; set; } = "";
}
