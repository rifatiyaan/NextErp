namespace NextErp.Domain.Entities
{
    /// <summary>Recorded payment against a sale (supports partial payments).</summary>
    public class SalePayment : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "Payment";

        public Guid SaleId { get; set; }
        public Sale Sale { get; set; } = null!;

        public decimal Amount { get; set; }
        public PaymentMethodType PaymentMethod { get; set; }
        public DateTime PaidAt { get; set; }
        public string? Reference { get; set; }

        public Guid TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
