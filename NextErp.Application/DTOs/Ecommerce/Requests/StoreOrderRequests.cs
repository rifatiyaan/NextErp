namespace NextErp.Application.DTOs.Ecommerce;

public sealed record StoreOrderItemRequest(int ProductVariantId, decimal Quantity);

public sealed class StoreOrderCreateRequest
{
    public string CustomerName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Note { get; set; }
    public List<StoreOrderItemRequest> Items { get; set; } = new();

    // Honeypot: humans never see it, bots fill it. Checked in the controller.
    public string? Website { get; set; }
}
