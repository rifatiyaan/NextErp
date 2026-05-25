using NextErp.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SaleDto = NextErp.Application.DTOs.Sale;

namespace NextErp.Application.Services;

/// <summary>
/// QuestPDF-backed invoice renderer. Layout is intentionally minimal — header,
/// customer block, line items, totals, payments, footer. Customise the visual
/// chrome here as branding evolves; the controller and frontend don't need to
/// know about layout changes.
/// </summary>
public sealed class InvoicePdfService : IInvoicePdfService
{
    public Task<byte[]> RenderSaleInvoiceAsync(
        SaleDto.Response.Get.Single sale,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sale);

        // QuestPDF's GeneratePdf() is synchronous + CPU-bound. Wrap in
        // Task.FromResult so the public API is async-ready for future
        // background-job dispatch without breaking call sites.
        var bytes = Document.Create(container => ComposeDocument(container, sale))
            .GeneratePdf();

        return Task.FromResult(bytes);
    }

    private static void ComposeDocument(IDocumentContainer container, SaleDto.Response.Get.Single sale)
    {
        container.Page(page =>
        {
            page.Margin(36);
            page.Size(PageSizes.A4);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(t => t.FontSize(10).FontColor(Colors.Grey.Darken4));

            page.Header().Element(header => ComposeHeader(header, sale));
            page.Content().PaddingVertical(12).Element(content => ComposeContent(content, sale));
            page.Footer().Element(ComposeFooter);
        });
    }

    private static void ComposeHeader(IContainer container, SaleDto.Response.Get.Single sale)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("INVOICE").FontSize(22).Bold().FontColor(Colors.Grey.Darken4);
                col.Item().Text($"# {sale.SaleNumber}").FontSize(11).FontColor(Colors.Grey.Medium);
            });

            row.ConstantItem(180).AlignRight().Column(col =>
            {
                // Tenant branding lives in SystemSettings — the future hook is to
                // pass an ICompanyProfile here. For now keep a static placeholder
                // so the layout slot is reserved.
                col.Item().Text("NextErp").FontSize(13).Bold();
                col.Item().Text("Sales receipt").FontSize(9).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private static void ComposeContent(IContainer container, SaleDto.Response.Get.Single sale)
    {
        container.Column(column =>
        {
            column.Spacing(16);
            column.Item().Element(meta => ComposeMetaBlock(meta, sale));
            column.Item().Element(items => ComposeLineItems(items, sale));
            column.Item().Element(totals => ComposeTotals(totals, sale));

            if (sale.Payments.Count > 0)
                column.Item().Element(payments => ComposePayments(payments, sale));

            if (!string.IsNullOrWhiteSpace(sale.Metadata.Notes))
                column.Item().Element(notes => ComposeNotes(notes, sale.Metadata.Notes!));
        });
    }

    private static void ComposeMetaBlock(IContainer container, SaleDto.Response.Get.Single sale)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Bill to").FontSize(9).FontColor(Colors.Grey.Medium).LetterSpacing(0.05f);
                col.Item().Text(string.IsNullOrWhiteSpace(sale.CustomerName) ? "Walk-in customer" : sale.CustomerName)
                    .FontSize(11)
                    .Bold();
            });

            row.ConstantItem(160).Column(col =>
            {
                col.Item().Text("Issue date").FontSize(9).FontColor(Colors.Grey.Medium).LetterSpacing(0.05f);
                col.Item().Text(sale.SaleDate.ToString("yyyy-MM-dd")).FontSize(11);
            });

            row.ConstantItem(120).AlignRight().Column(col =>
            {
                var status = ResolveStatus(sale);
                col.Item().Text("Status").FontSize(9).FontColor(Colors.Grey.Medium).LetterSpacing(0.05f);
                col.Item().Text(status).FontSize(11).Bold().FontColor(StatusColor(status));
            });
        });
    }

    private static void ComposeLineItems(IContainer container, SaleDto.Response.Get.Single sale)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(28);   // #
                columns.RelativeColumn(4);    // Item
                columns.RelativeColumn(2);    // SKU
                columns.RelativeColumn(1.2f); // Qty
                columns.RelativeColumn(1.6f); // Unit
                columns.RelativeColumn(1.6f); // Total
            });

            table.Header(header =>
            {
                static IContainer HeaderCell(IContainer cell) => cell
                    .DefaultTextStyle(t => t.SemiBold().FontSize(9).FontColor(Colors.Grey.Darken3))
                    .PaddingVertical(6)
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten2);

                header.Cell().Element(HeaderCell).Text("#");
                header.Cell().Element(HeaderCell).Text("Item");
                header.Cell().Element(HeaderCell).Text("SKU");
                header.Cell().Element(HeaderCell).AlignRight().Text("Qty");
                header.Cell().Element(HeaderCell).AlignRight().Text("Unit");
                header.Cell().Element(HeaderCell).AlignRight().Text("Total");
            });

            for (var index = 0; index < sale.Items.Count; index++)
            {
                var item = sale.Items[index];

                static IContainer BodyCell(IContainer cell) => cell
                    .PaddingVertical(5)
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten4);

                table.Cell().Element(BodyCell).Text((index + 1).ToString());
                table.Cell().Element(BodyCell).Column(c =>
                {
                    c.Item().Text(item.ProductTitle).FontSize(10);
                    if (!string.IsNullOrWhiteSpace(item.VariantTitle) && item.VariantTitle != item.ProductTitle)
                    {
                        c.Item().Text(item.VariantTitle).FontSize(8).FontColor(Colors.Grey.Medium);
                    }
                });
                table.Cell().Element(BodyCell).Text(item.VariantSku ?? "—").FontColor(Colors.Grey.Darken1);
                table.Cell().Element(BodyCell).AlignRight().Text(FormatQuantity(item.Quantity));
                table.Cell().Element(BodyCell).AlignRight().Text(FormatMoney(item.UnitPrice));
                table.Cell().Element(BodyCell).AlignRight().Text(FormatMoney(item.Total)).SemiBold();
            }
        });
    }

    private static void ComposeTotals(IContainer container, SaleDto.Response.Get.Single sale)
    {
        container.AlignRight().Column(col =>
        {
            col.Spacing(2);
            col.Item().Width(220).Element(line => Line(line, "Subtotal", FormatMoney(sale.TotalAmount)));

            if (sale.Discount > 0)
                col.Item().Width(220).Element(line =>
                    Line(line, "Discount", $"-{FormatMoney(sale.Discount)}"));

            if (sale.Tax > 0)
                col.Item().Width(220).Element(line => Line(line, "Tax", FormatMoney(sale.Tax)));

            col.Item().Width(220).PaddingTop(4).BorderTop(1).BorderColor(Colors.Grey.Darken1)
                .PaddingTop(4).Element(line => Line(line, "Total due", FormatMoney(sale.FinalAmount), bold: true));

            if (sale.TotalPaid > 0)
                col.Item().Width(220).PaddingTop(4)
                    .Element(line => Line(line, "Paid", FormatMoney(sale.TotalPaid)));

            if (sale.BalanceDue > 0)
                col.Item().Width(220)
                    .Element(line =>
                        Line(line, "Balance due", FormatMoney(sale.BalanceDue), bold: true,
                            color: Colors.Red.Darken1));
        });

        static void Line(IContainer container, string label, string amount, bool bold = false, string? color = null)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    var span = text.Span(label);
                    if (bold) span.SemiBold();
                    if (color != null) span.FontColor(color);
                });
                row.ConstantItem(120).AlignRight().Text(text =>
                {
                    var span = text.Span(amount);
                    span.FontFamily(Fonts.Consolas);
                    if (bold) span.SemiBold();
                    if (color != null) span.FontColor(color);
                });
            });
        }
    }

    private static void ComposePayments(IContainer container, SaleDto.Response.Get.Single sale)
    {
        container.Column(col =>
        {
            col.Item().Text("Payments").SemiBold().FontSize(11);
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2); // Date
                    columns.RelativeColumn(2); // Method
                    columns.RelativeColumn(3); // Reference
                    columns.RelativeColumn(2); // Amount
                });

                static IContainer PHeader(IContainer c) => c.PaddingVertical(4).BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .DefaultTextStyle(t => t.SemiBold().FontSize(9).FontColor(Colors.Grey.Darken3));

                table.Header(h =>
                {
                    h.Cell().Element(PHeader).Text("Date");
                    h.Cell().Element(PHeader).Text("Method");
                    h.Cell().Element(PHeader).Text("Reference");
                    h.Cell().Element(PHeader).AlignRight().Text("Amount");
                });

                foreach (var p in sale.Payments)
                {
                    static IContainer PBody(IContainer c) => c.PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten4);

                    table.Cell().Element(PBody).Text(p.PaidAt.ToString("yyyy-MM-dd"));
                    table.Cell().Element(PBody).Text(p.PaymentMethod.ToString());
                    table.Cell().Element(PBody).Text(p.Reference ?? "—").FontColor(Colors.Grey.Darken1);
                    table.Cell().Element(PBody).AlignRight().Text(FormatMoney(p.Amount)).SemiBold();
                }
            });
        });
    }

    private static void ComposeNotes(IContainer container, string notes)
    {
        container.Background(Colors.Grey.Lighten4).Padding(8).Column(col =>
        {
            col.Item().Text("Notes").SemiBold().FontSize(9).FontColor(Colors.Grey.Darken3);
            col.Item().Text(notes).FontSize(9);
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

    // ----- formatting helpers -------------------------------------------------

    private static string FormatMoney(decimal value) => value.ToString("N2");

    private static string FormatQuantity(decimal q)
    {
        // Drop trailing zeros for whole units (3.00 → 3) but keep precision for
        // fractional like 1.5kg.
        return q == decimal.Truncate(q) ? q.ToString("N0") : q.ToString("0.###");
    }

    private static string ResolveStatus(SaleDto.Response.Get.Single sale)
    {
        if (sale.BalanceDue <= 0.005m) return "Paid";
        if (sale.TotalPaid <= 0.005m) return "Due";
        return "Partial";
    }

    private static string StatusColor(string status) => status switch
    {
        "Paid" => Colors.Green.Darken1,
        "Partial" => Colors.Orange.Darken2,
        "Due" => Colors.Red.Darken1,
        _ => Colors.Grey.Darken3,
    };
}
