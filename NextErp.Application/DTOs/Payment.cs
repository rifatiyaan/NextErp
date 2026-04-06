using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs
{
    public partial class Payment
    {
        public class Request
        {
            public class Record
            {
                public Guid SaleId { get; set; }
                public decimal Amount { get; set; }
                public PaymentMethodType PaymentMethod { get; set; }
                public DateTime? PaidAt { get; set; }
                public string? Reference { get; set; }
            }
        }

        public class Response
        {
            public class Line
            {
                public Guid Id { get; set; }
                public Guid SaleId { get; set; }
                public decimal Amount { get; set; }
                public PaymentMethodType PaymentMethod { get; set; }
                public DateTime PaidAt { get; set; }
                public string? Reference { get; set; }
                public DateTime CreatedAt { get; set; }
            }
        }
    }
}
