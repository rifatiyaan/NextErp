using NextErp.Application.Common.Settings;

namespace NextErp.Application.Settings;

[SettingsModule("Sales", "Sales & Checkout")]
public sealed class SalesSettings
{
    [Setting(
        description: "Adds a separate store-level VAT line on top of the cart subtotal. Product prices are already VAT-inclusive — leave off unless the store charges an additional regional VAT overlay.",
        displayName: "Add store VAT at checkout")]
    public bool StoreVatEnabled { get; set; } = false;

    [Setting(
        description: "Percent of taxable amount added as the store VAT line. Used only when 'Add store VAT' is on. Norwegian standard rate is 25%.",
        displayName: "Store VAT rate (%)")]
    [SettingRange(0, 100)]
    public decimal StoreVatPercent { get; set; } = 0m;

    [Setting(
        description: "Live promotion preview in /sales/create while building a cart. Off = totals only update on submit.",
        displayName: "Enable live pricing preview")]
    public bool EnablePricingPreview { get; set; } = true;
}
