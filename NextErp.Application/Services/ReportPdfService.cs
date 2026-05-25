using NextErp.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SaleDto = NextErp.Application.DTOs.Sale;
using ReportDto = NextErp.Application.DTOs.Report;

namespace NextErp.Application.Services;

/// <summary>
/// QuestPDF-backed renderer for the three Phase B reports. All three share
/// the same chrome (page size, header band, footer); the body shape differs.
/// Per-report Compose methods use local helper closures to keep cell wiring
/// terse without introducing a typed shortcut over QuestPDF's
/// TableDescriptor / TableCellDescriptor hierarchy.
/// </summary>
public sealed class ReportPdfService : IReportPdfService
{
    public Task<byte[]> RenderSalesReportAsync(
        SaleDto.Response.Get.Report report,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);
        var bytes = Document.Create(c => ComposePage(c, "Sales Report",
            $"{report.StartDate:yyyy-MM-dd} → {report.EndDate:yyyy-MM-dd}",
            content => ComposeSalesBody(content, report)))
            .GeneratePdf();
        return Task.FromResult(bytes);
    }

    public Task<byte[]> RenderStockValuationReportAsync(
        ReportDto.Response.StockValuation report,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);
        var bytes = Document.Create(c => ComposePage(c, "Stock Valuation Report",
            $"As of {report.AsOf:yyyy-MM-dd HH:mm} UTC",
            content => ComposeStockValuationBody(content, report)))
            .GeneratePdf();
        return Task.FromResult(bytes);
    }

    public Task<byte[]> RenderProfitMarginReportAsync(
        ReportDto.Response.ProfitMargin report,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);
        var bytes = Document.Create(c => ComposePage(c, "Profit Margin Report",
            $"{report.StartDate:yyyy-MM-dd} → {report.EndDate:yyyy-MM-dd}",
            content => ComposeProfitMarginBody(content, report)))
            .GeneratePdf();
        return Task.FromResult(bytes);
    }

    // ----- shared chrome -----------------------------------------------------

    private static void ComposePage(
        IDocumentContainer container,
        string title,
        string subtitle,
        Action<IContainer> renderBody)
    {
        container.Page(page =>
        {
            page.Margin(36);
            page.Size(PageSizes.A4);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(t => t.FontSize(10).FontColor(Colors.Grey.Darken4));

            page.Header().Element(h => ComposeHeader(h, title, subtitle));
            page.Content().PaddingVertical(12).Element(renderBody);
            page.Footer().Element(ComposeFooter);
        });
    }

    private static void ComposeHeader(IContainer container, string title, string subtitle)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(title).FontSize(20).Bold();
                col.Item().Text(subtitle).FontSize(10).FontColor(Colors.Grey.Medium);
            });
            row.ConstantItem(120).AlignRight().Column(col =>
            {
                col.Item().Text("NextErp").FontSize(13).Bold();
                col.Item().Text("Report").FontSize(9).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span($"Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(8).FontColor(Colors.Grey.Medium);
            text.Span("  ·  ").FontSize(8).FontColor(Colors.Grey.Medium);
            text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
            text.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
            text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }

    private static IContainer HeaderCellStyle(IContainer cell) => cell
        .DefaultTextStyle(t => t.SemiBold().FontSize(9).FontColor(Colors.Grey.Darken3))
        .PaddingVertical(6)
        .BorderBottom(1)
        .BorderColor(Colors.Grey.Lighten2);

    private static IContainer BodyCellStyle(IContainer cell) => cell
        .PaddingVertical(4)
        .BorderBottom(1)
        .BorderColor(Colors.Grey.Lighten4);

    // ----- Sales report body -------------------------------------------------

    private static void ComposeSalesBody(IContainer container, SaleDto.Response.Get.Report report)
    {
        container.Column(col =>
        {
            col.Spacing(12);
            col.Item().Element(s => SummaryStrip(s, new (string, string)[]
            {
                ("Sales count", report.TotalSales.ToString("N0")),
                ("Total revenue", FormatMoney(report.TotalSalesAmount)),
                ("Avg sale", FormatMoney(report.TotalSales > 0 ? report.TotalSalesAmount / report.TotalSales : 0)),
            }));

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(3);
                    c.RelativeColumn(1.5f);
                    c.RelativeColumn(1.5f);
                    c.RelativeColumn(1.5f);
                });

                table.Header(header =>
                {
                    void H(string text) => header.Cell().Element(HeaderCellStyle).Text(text);
                    void HR(string text) => header.Cell().Element(HeaderCellStyle).AlignRight().Text(text);
                    H("Sale #");
                    H("Date");
                    H("Customer");
                    HR("Final");
                    HR("Paid");
                    HR("Due");
                });

                void B(string text) => table.Cell().Element(BodyCellStyle).Text(text);
                void BR(string text) => table.Cell().Element(BodyCellStyle).AlignRight().Text(text);

                foreach (var sale in report.Sales.OrderByDescending(s => s.SaleDate))
                {
                    B(sale.SaleNumber);
                    B(sale.SaleDate.ToString("yyyy-MM-dd"));
                    B(string.IsNullOrWhiteSpace(sale.CustomerName) ? "Walk-in" : sale.CustomerName);
                    BR(FormatMoney(sale.FinalAmount));
                    BR(FormatMoney(sale.TotalPaid));
                    BR(FormatMoney(sale.BalanceDue));
                }
            });
        });
    }

    // ----- Stock valuation body ---------------------------------------------

    private static void ComposeStockValuationBody(IContainer container, ReportDto.Response.StockValuation report)
    {
        container.Column(col =>
        {
            col.Spacing(12);
            col.Item().Element(s => SummaryStrip(s, new (string, string)[]
            {
                ("Products", report.ProductCount.ToString("N0")),
                ("Total qty", FormatQty(report.TotalQuantity)),
                ("Total value", FormatMoney(report.TotalValue)),
            }));

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(1.2f);
                    c.RelativeColumn(1.5f);
                    c.RelativeColumn(1.7f);
                });

                table.Header(header =>
                {
                    void H(string text) => header.Cell().Element(HeaderCellStyle).Text(text);
                    void HR(string text) => header.Cell().Element(HeaderCellStyle).AlignRight().Text(text);
                    H("Product");
                    H("SKU");
                    H("Category");
                    HR("Qty");
                    HR("Unit cost");
                    HR("Value");
                });

                void B(string text) => table.Cell().Element(BodyCellStyle).Text(text);
                void BR(string text) => table.Cell().Element(BodyCellStyle).AlignRight().Text(text);

                foreach (var line in report.Lines)
                {
                    B(line.ProductTitle);
                    B(line.VariantSku ?? "—");
                    B(line.Category ?? "—");
                    BR(FormatQty(line.Quantity));
                    BR(FormatMoney(line.UnitCost));
                    BR(FormatMoney(line.Value));
                }
            });
        });
    }

    // ----- Profit margin body -----------------------------------------------

    private static void ComposeProfitMarginBody(IContainer container, ReportDto.Response.ProfitMargin report)
    {
        container.Column(col =>
        {
            col.Spacing(12);
            col.Item().Element(s => SummaryStrip(s, new (string, string)[]
            {
                ("Sales", report.SaleCount.ToString("N0")),
                ("Revenue", FormatMoney(report.TotalRevenue)),
                ("Cost", FormatMoney(report.TotalCost)),
                ("Profit", FormatMoney(report.TotalProfit)),
                ("Avg margin", $"{report.AverageMarginPercent:N2}%"),
            }));

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(3);
                    c.RelativeColumn(1.5f);
                    c.RelativeColumn(1.5f);
                    c.RelativeColumn(1.5f);
                    c.RelativeColumn(1.2f);
                });

                table.Header(header =>
                {
                    void H(string text) => header.Cell().Element(HeaderCellStyle).Text(text);
                    void HR(string text) => header.Cell().Element(HeaderCellStyle).AlignRight().Text(text);
                    H("Sale #");
                    H("Date");
                    H("Customer");
                    HR("Revenue");
                    HR("Cost");
                    HR("Profit");
                    HR("Margin %");
                });

                void B(string text) => table.Cell().Element(BodyCellStyle).Text(text);
                void BR(string text) => table.Cell().Element(BodyCellStyle).AlignRight().Text(text);

                foreach (var line in report.Lines)
                {
                    B(line.SaleNumber);
                    B(line.SaleDate.ToString("yyyy-MM-dd"));
                    B(string.IsNullOrWhiteSpace(line.CustomerName) ? "Walk-in" : line.CustomerName);
                    BR(FormatMoney(line.Revenue));
                    BR(FormatMoney(line.Cost));
                    BR(FormatMoney(line.Profit));
                    BR($"{line.MarginPercent:N2}%");
                }
            });
        });
    }

    // ----- shared layout helpers --------------------------------------------

    private static void SummaryStrip(IContainer container, IReadOnlyList<(string Label, string Value)> stats)
    {
        container.Row(row =>
        {
            foreach (var (label, value) in stats)
            {
                row.RelativeItem().PaddingHorizontal(4).Column(col =>
                {
                    col.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Medium).LetterSpacing(0.05f);
                    col.Item().Text(value).FontSize(13).SemiBold();
                });
            }
        });
    }

    private static string FormatMoney(decimal value) => value.ToString("N2");

    private static string FormatQty(decimal value)
        => value == decimal.Truncate(value) ? value.ToString("N0") : value.ToString("0.###");
}
