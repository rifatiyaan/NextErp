namespace NextErp.Application.DTOs
{
    public partial class Sale
    {
        public partial class Request
        {
            public partial class Get
            {
                public class Single
                {
                    public Guid Id { get; set; }
                }

                public class Bulk
                {
                    public int Page { get; set; } = 1;
                    public int PageSize { get; set; } = 10;
                    public string? SearchTerm { get; set; }
                    public Guid? CustomerId { get; set; }
                    public DateTime? StartDate { get; set; }
                    public DateTime? EndDate { get; set; }
                    public string? SortBy { get; set; }
                    public bool SortDescending { get; set; }
                }

                public class Report
                {
                    public DateTime StartDate { get; set; }
                    public DateTime EndDate { get; set; }
                    public Guid? CustomerId { get; set; }
                }
            }

            public partial class Create
            {
                public class Single
                {
                    public string Title { get; set; } = null!;
                    public string SaleNumber { get; set; } = null!;
                    public Guid CustomerId { get; set; }
                    public DateTime SaleDate { get; set; }
                    public List<SaleItemRequest> Items { get; set; } = new();
                    public Metadata Metadata { get; set; } = new();
                }

                public class SaleItemRequest
                {
                    public string Title { get; set; } = null!;
                    public int ProductId { get; set; }
                    public decimal Quantity { get; set; }
                    public decimal UnitPrice { get; set; }
                }
            }

            public class Metadata
            {
                public string? ReferenceNo { get; set; }
                public string? PaymentMethod { get; set; }
                public string? Notes { get; set; }
            }
        }

        public partial class Response
        {
            public partial class Get
            {
                public class Single
                {
                    public Guid Id { get; set; }
                    public string Title { get; set; } = null!;
                    public string SaleNumber { get; set; } = null!;
                    public Guid CustomerId { get; set; }
                    public string CustomerName { get; set; } = null!;
                    public DateTime SaleDate { get; set; }
                    public decimal TotalAmount { get; set; }
                    public List<SaleItemResponse> Items { get; set; } = new();
                    public Request.Metadata Metadata { get; set; } = new();
                    public bool IsActive { get; set; }
                    public DateTime CreatedAt { get; set; }
                    public DateTime? UpdatedAt { get; set; }
                    public Guid TenantId { get; set; }
                    public Guid? BranchId { get; set; }
                }

                public class SaleItemResponse
                {
                    public Guid Id { get; set; }
                    public string Title { get; set; } = null!;
                    public int ProductId { get; set; }
                    public string ProductTitle { get; set; } = null!;
                    public decimal Quantity { get; set; }
                    public decimal UnitPrice { get; set; }
                    public decimal Total { get; set; }
                }

                public class Bulk
                {
                    public List<Single> Sales { get; set; } = new();
                    public int TotalCount { get; set; }
                    public int Page { get; set; }
                    public int PageSize { get; set; }
                    public int TotalPages { get; set; }
                }

                public class Report
                {
                    public List<Single> Sales { get; set; } = new();
                    public decimal TotalSalesAmount { get; set; }
                    public int TotalSales { get; set; }
                    public DateTime StartDate { get; set; }
                    public DateTime EndDate { get; set; }
                }
            }

            public partial class Create
            {
                public class Single
                {
                    public Guid Id { get; set; }
                    public string Title { get; set; } = null!;
                    public string SaleNumber { get; set; } = null!;
                    public decimal TotalAmount { get; set; }
                    public DateTime CreatedAt { get; set; }
                }
            }
        }
    }
}
