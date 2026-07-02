using NextErp.Application.DTOs.Purchase;

namespace NextErp.Application.Interfaces;

/// <summary>
/// Generates a printable PDF for a single purchase invoice. Mirrors
/// <see cref="IInvoicePdfService"/> for sales — caller passes a hydrated
/// <see cref="PurchaseResponse"/> (with line items + metadata)
/// and gets back the rendered bytes.
/// </summary>
public interface IPurchaseInvoicePdfService
{
    Task<byte[]> RenderPurchaseInvoiceAsync(
        PurchaseResponse purchase,
        CancellationToken cancellationToken = default);
}
