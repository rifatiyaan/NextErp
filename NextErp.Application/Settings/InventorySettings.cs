using NextErp.Application.Common.Settings;

namespace NextErp.Application.Settings;

public enum InventoryConsumptionOrder
{
    Single = 0,
    Fifo = 1,
    Lifo = 2,
}

[SettingsModule("Inventory")]
public sealed class InventorySettings
{
    [Setting(
        description: "How stock is consumed on sale. Single keeps the current 'one quantity per variant' behavior. FIFO/LIFO require batch-tracked stock (planned).",
        displayName: "Stock consumption order")]
    public InventoryConsumptionOrder ConsumptionOrder { get; set; } = InventoryConsumptionOrder.Single;

    [Setting(
        description: "Show the product thumbnail in list views. Off keeps rows compact for text-heavy workflows.",
        displayName: "Show product image in list")]
    public bool ShowProductImageInList { get; set; } = true;
}
