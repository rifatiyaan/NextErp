namespace NextErp.Application.DTOs.Ecommerce;

public sealed record OnlineOrderRow(int Id, string OrderNumber, string CustomerName, string Phone, int ItemCount, decimal ItemsTotal, decimal DeliveryFee, string Status, DateTime CreatedAt);
public sealed record PagedOnlineOrdersResponse(int Total, List<OnlineOrderRow> Data);
public sealed record OnlineOrderItemRow(string ProductTitle, string Sku, decimal UnitPrice, decimal Quantity, decimal LineTotal);
public sealed record OnlineOrderDetailResponse(int Id, string OrderNumber, string CustomerName, string Phone, string Address, string? Note, string Status, string? CancelReason, decimal DeliveryFee, Guid? PartyId, Guid? SaleId, DateTime CreatedAt, DateTime? ConfirmedAt, List<OnlineOrderItemRow> Items);
