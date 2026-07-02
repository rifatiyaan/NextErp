using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs.Payment;

public sealed record PaymentLineResponse
{
    public Guid Id { get; init; }
    public Guid SaleId { get; init; }
    public decimal Amount { get; init; }
    public PaymentMethodType PaymentMethod { get; init; }
    public DateTime PaidAt { get; init; }
    public string? Reference { get; init; }
    public DateTime CreatedAt { get; init; }
}
