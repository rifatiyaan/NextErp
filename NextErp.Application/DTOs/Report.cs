namespace NextErp.Application.DTOs;

/// <summary>
/// Aggregate report DTOs surfaced by the Reports module. Each Response shape
/// is independently consumable by both the JSON endpoints (charting in the
/// dashboard, ad-hoc API consumers) and the QuestPDF renderer.
/// </summary>
public partial class Report
{
    public partial class Response
    {
        public partial class StockValuation
        {
            public DateTime AsOf { get; set; }
            public int ProductCount { get; set; }
            public decimal TotalQuantity { get; set; }
            public decimal TotalValue { get; set; }
            public List<Line> Lines { get; set; } = new();

            public class Line
            {
                public int ProductId { get; set; }
                public string ProductTitle { get; set; } = null!;
                public string? VariantSku { get; set; }
                public string? Category { get; set; }
                public decimal Quantity { get; set; }
                public decimal UnitCost { get; set; }
                public decimal Value { get; set; }
            }
        }

        public partial class ProfitMargin
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int SaleCount { get; set; }
            public decimal TotalRevenue { get; set; }
            public decimal TotalCost { get; set; }
            public decimal TotalProfit { get; set; }
            public decimal AverageMarginPercent { get; set; }
            public List<Line> Lines { get; set; } = new();

            /// <summary>
            /// One line per sale. We aggregate per-sale rather than per-line-item
            /// so the report stays a manageable size at high transaction volumes;
            /// drill-down into individual items is a separate report.
            /// </summary>
            public class Line
            {
                public Guid SaleId { get; set; }
                public string SaleNumber { get; set; } = null!;
                public string CustomerName { get; set; } = null!;
                public DateTime SaleDate { get; set; }
                public decimal Revenue { get; set; }
                public decimal Cost { get; set; }
                public decimal Profit { get; set; }
                public decimal MarginPercent { get; set; }
            }
        }
    }
}
