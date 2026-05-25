using NextErp.Application.Common.Settings;

namespace NextErp.Application.Settings;

[SettingsModule("UI", "User Interface")]
public sealed class UISettings
{
    [Setting(
        description: "Default page size for list views (sales, purchases, parties, etc.). Operators can still change it per session.",
        displayName: "Default list page size")]
    [SettingRange(5, 200)]
    public int DefaultListPageSize { get; set; } = 10;
}
