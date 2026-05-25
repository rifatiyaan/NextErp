using NextErp.Application.Common.Settings;

namespace NextErp.Application.Settings;

public enum CurrencyLocale
{
    EnUS = 0,
    NbNO = 1,
}

[SettingsModule("Locale")]
public sealed class LocaleSettings
{
    [Setting(
        description: "Locale used by money/number formatters across the app. Affects thousands/decimal separators and currency symbol placement.",
        displayName: "Currency locale")]
    public CurrencyLocale CurrencyLocale { get; set; } = CurrencyLocale.EnUS;
}
