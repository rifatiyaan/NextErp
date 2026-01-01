namespace NextErp.Application.DTOs
{
    public partial class Stock
    {
        public partial class Response
        {
            public class Single
            {
                public int Id { get; set; }
                public int ProductId { get; set; }
                public string ProductTitle { get; set; } = null!;
                public string ProductCode { get; set; } = null!;
                public decimal AvailableQuantity { get; set; }
                public DateTime CreatedAt { get; set; }
                public DateTime? UpdatedAt { get; set; }
                public Guid TenantId { get; set; }
                public Guid? BranchId { get; set; }
            }

            public class CurrentStockReport
            {
                public List<Single> Stocks { get; set; } = new();
                public int TotalProducts { get; set; }
                public decimal TotalQuantity { get; set; }
            }

            public class LowStockReport
            {
                public List<LowStockItem> Items { get; set; } = new();
                public int TotalLowStockProducts { get; set; }
            }

            public class LowStockItem
            {
                public int ProductId { get; set; }
                public string ProductTitle { get; set; } = null!;
                public string ProductCode { get; set; } = null!;
                public decimal AvailableQuantity { get; set; }
                public int? ReorderLevel { get; set; }
                public string Status { get; set; } = null!; // "Low", "Critical", "Out of Stock"
            }
        }
    }
}
