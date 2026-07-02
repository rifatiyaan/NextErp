using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs.Payment;

public sealed record RecordPaymentRequest
{
    public Guid SaleId { get; init; }
    public decimal Amount { get; init; }
    public PaymentMethodType PaymentMethod { get; init; }
    public DateTime? PaidAt { get; init; }
    public string? Reference { get; init; }
}
