using NextErp.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PurchaseDto = NextErp.Application.DTOs.Purchase;

namespace NextErp.Application.Services;

/// <summary>
/// QuestPDF-backed purchase invoice renderer. Layout mirrors the sales
/// invoice (header → meta → line items → totals → optional notes → footer)
/// but drops the payments table since purchase records don't track partial
/// payments today, and renames a few labels (Supplier, Unit cost, Net total).
/// </summary>
public sealed class PurchaseInvoicePdfService : IPurchaseInvoicePdfService
{
    public Task<byte[]> RenderPurchaseInvoiceAsync(
        PurchaseDto.Response.Get.Single purchase,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(purchase);

        var bytes = Document.Create(container => ComposeDocument(container, purchase))
            .GeneratePdf();

        return Task.FromResult(bytes);
    }

    private static void ComposeDocument(IDocumentContainer container, PurchaseDto.Response.Get.Single purchase)
    {
        container.Page(page =>
        {
            page.Margin(36);
            page.Size(PageSizes.A4);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(t => t.FontSize(10).FontColor(Colors.Grey.Darken4));

            page.Header().Element(header => ComposeHeader(header, purchase));
            page.Content().PaddingVertical(12).Element(content => ComposeContent(content, purchase));
            page.Footer().Element(ComposeFooter);
        });
    }

    private static void ComposeHeader(IContainer container, PurchaseDto.Response.Get.Single purchase)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("PURCHASE").FontSize(22).Bold().FontColor(Colors.Grey.Darken4);
                col.Item().Text($"# {purchase.PurchaseNumber}").FontSize(11).FontColor(Colors.Grey.Medium);
            });

            row.ConstantItem(180).AlignRight().Column(col =>
            {
                col.Item().Text("NextErp").FontSize(13).Bold();
                col.Item().Text("Purchase invoice").FontSize(9).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private static void ComposeContent(IContainer container, PurchaseDto.Response.Get.Single purchase)
    {
        container.Column(column =>
        {
            column.Spacing(16);
            column.Item().Element(meta => ComposeMetaBlock(meta, purchase));
            column.Item().Element(items => ComposeLineItems(items, purchase));
            column.Item().Element(totals => ComposeTotals(totals, purchase));

            // Reference numbers (bill / batch / challan / external ref) — small
            // grid below the totals for traceability.
            column.Item().Element(refs => ComposeReferences(refs, purchase));

            if (!string.IsNullOrWhiteSpace(purchase.Metadata.Notes))
                column.Item().Element(notes => ComposeNotes(notes, purchase.Metadata.Notes!));
        });
    }

    private static void ComposeMetaBlock(IContainer container, PurchaseDto.Response.Get.Single purchase)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Supplier").FontSize(9).FontColor(Colors.Grey.Medium).LetterSpacing(0.05f);
                col.Item().Text(string.IsNullOrWhiteSpace(purchase.SupplierName) ? "—" : purchase.SupplierName)
                    .FontSize(11)
                    .Bold();
            });

            row.ConstantItem(160).Column(col =>
            {
                col.Item().Text("Issue date").FontSize(9).FontColor(Colors.Grey.Medium).LetterSpacing(0.05f);
                col.Item().Text(purchase.PurchaseDate.ToString("yyyy-MM-dd")).FontSize(11);
            });

            row.ConstantItem(120).AlignRight().Column(col =>
            {
                col.Item().Text("Status").FontSize(9).FontColor(Colors.Grey.Medium).LetterSpacing(0.05f);
                col.Item()
                    .Text(purchase.IsActive ? "Active" : "Inactive")
                    .FontSize(11)
                    .Bold()
                    .FontColor(purchase.IsActive ? Colors.Green.Darken1 : Colors.Grey.Darken2);
            });
        });
    }

    private static void ComposeLineItems(IContainer container, PurchaseDto.Response.Get.Single purchase)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(28);   // #
                columns.RelativeColumn(4);    // Item
                columns.RelativeColumn(2);    // SKU
                columns.RelativeColumn(1.2f); // Qty
                columns.RelativeColumn(1.6f); // Unit cost
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
                header.Cell().Element(HeaderCell).AlignRight().Text("Unit cost");
                header.Cell().Element(HeaderCell).AlignRight().Text("Total");
            });

            for (var index = 0; index < purchase.Items.Count; index++)
            {
                var item = purchase.Items[index];

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
                table.Cell().Element(BodyCell).AlignRight().Text(FormatMoney(item.UnitCost));
                table.Cell().Element(BodyCell).AlignRight().Text(FormatMoney(item.Total)).SemiBold();
            }
        });
    }

    private static void ComposeTotals(IContainer container, PurchaseDto.Response.Get.Single purchase)
    {
        container.AlignRight().Column(col =>
        {
            col.Spacing(2);
            col.Item().Width(220).Element(line => Line(line, "Subtotal", FormatMoney(purchase.TotalAmount)));

            if (purchase.Discount > 0)
                col.Item().Width(220).Element(line =>
                    Line(line, "Discount", $"-{FormatMoney(purchase.Discount)}"));

            col.Item().Width(220).PaddingTop(4).BorderTop(1).BorderColor(Colors.Grey.Darken1).PaddingTop(4)
                .Element(line => Line(line, "Net total", FormatMoney(purchase.NetTotal), bold: true));
        });

        static void Line(IContainer container, string label, string amount, bool bold = false)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    var span = text.Span(label);
                    if (bold) span.SemiBold();
                });
                row.ConstantItem(120).AlignRight().Text(text =>
                {
                    var span = text.Span(amount);
                    span.FontFamily(Fonts.Consolas);
                    if (bold) span.SemiBold();
                });
            });
        }
    }

    private static void ComposeReferences(IContainer container, PurchaseDto.Response.Get.Single purchase)
    {
        var meta = purchase.Metadata;
        // Skip the section entirely if no reference field is populated — keeps
        // the page clean for cash-and-carry style purchases without paperwork.
        if (string.IsNullOrWhiteSpace(meta.BillNo)
            && string.IsNullOrWhiteSpace(meta.BatchNo)
            && string.IsNullOrWhiteSpace(meta.ChallanNo)
            && string.IsNullOrWhiteSpace(meta.ReferenceNo))
            return;

        container.Column(col =>
        {
            col.Item().Text("References").SemiBold().FontSize(11);
            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Element(c => Field(c, "Bill #", meta.BillNo));
                row.RelativeItem().Element(c => Field(c, "Batch #", meta.BatchNo));
                row.RelativeItem().Element(c => Field(c, "Challan #", meta.ChallanNo));
                row.RelativeItem().Element(c => Field(c, "Reference", meta.ReferenceNo));
            });
        });

        static void Field(IContainer container, string label, string? value)
        {
            container.Column(c =>
            {
                c.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Medium).LetterSpacing(0.05f);
                c.Item().Text(string.IsNullOrWhiteSpace(value) ? "—" : value!).FontSize(10);
            });
        }
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

    private static string FormatMoney(decimal value) => value.ToString("N2");

    private static string FormatQuantity(decimal q)
    {
        return q == decimal.Truncate(q) ? q.ToString("N0") : q.ToString("0.###");
    }
}
