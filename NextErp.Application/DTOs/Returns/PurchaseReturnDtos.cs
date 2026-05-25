namespace NextErp.Application.DTOs.Returns;

/// <summary>
/// DTOs for the Purchase Returns module. Mirror of <see cref="SaleReturnDto"/>
/// but anchored on Purchase rather than Sale, and named "TotalAmount" since
/// there's no refund/customer concept on the supplier side.
/// </summary>
public partial class PurchaseReturnDto
{
    public partial class Request
    {
        public partial class Create
        {
            public class Single
            {
                public Guid PurchaseId { get; set; }
                public DateTime? ReturnDate { get; set; }
                public string? Reason { get; set; }
                public string? Notes { get; set; }
                public List<Line> Items { get; set; } = new();
            }

            public class Line
            {
                public Guid PurchaseItemId { get; set; }
                public int ProductVariantId { get; set; }
                public decimal Quantity { get; set; }
                public decimal? UnitPrice { get; set; }
                public string? ConditionNote { get; set; }
            }
        }
    }

    public partial class Response
    {
        public partial class Get
        {
            public class Single
            {
                public Guid Id { get; set; }
                public string ReturnNumber { get; set; } = null!;
                public Guid PurchaseId { get; set; }
                public string PurchaseNumber { get; set; } = null!;
                public string? SupplierName { get; set; }
                public DateTime ReturnDate { get; set; }
                public string? Reason { get; set; }
                public string? Notes { get; set; }
                public decimal TotalAmount { get; set; }
                public bool IsActive { get; set; }
                public DateTime CreatedAt { get; set; }
                public List<Line> Items { get; set; } = new();
            }

            public class ListRow
            {
                public Guid Id { get; set; }
                public string ReturnNumber { get; set; } = null!;
                public Guid PurchaseId { get; set; }
                public string PurchaseNumber { get; set; } = null!;
                public string? SupplierName { get; set; }
                public DateTime ReturnDate { get; set; }
                public decimal TotalAmount { get; set; }
                public int ItemCount { get; set; }
            }

            public class Line
            {
                public Guid Id { get; set; }
                public Guid PurchaseItemId { get; set; }
                public int ProductVariantId { get; set; }
                public string ProductTitle { get; set; } = null!;
                public string? VariantSku { get; set; }
                public decimal Quantity { get; set; }
                public decimal UnitPrice { get; set; }
                public decimal Subtotal { get; set; }
                public string? ConditionNote { get; set; }
            }
        }

        public class Paged
        {
            public int Total { get; set; }
            public int TotalDisplay { get; set; }
            public List<Get.ListRow> Data { get; set; } = new();
        }
    }
}
