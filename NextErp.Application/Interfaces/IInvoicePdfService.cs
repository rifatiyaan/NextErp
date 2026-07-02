using SaleDto = NextErp.Application.DTOs.Sale;

namespace NextErp.Application.Interfaces;

/// <summary>
/// Generates a printable PDF for a single sale invoice. Pure layout — the
/// caller passes a fully-hydrated <see cref="SaleDto.SaleResponse"/>
/// (including line items + payments) and gets back the rendered bytes.
/// </summary>
public interface IInvoicePdfService
{
    /// <summary>
    /// Render the given sale to a PDF byte array. Synchronous on the underlying
    /// QuestPDF API; the wrapper is async so we can swap the backend (e.g. to
    /// a queued background job) without changing every call site.
    /// </summary>
    Task<byte[]> RenderSaleInvoiceAsync(
        SaleDto.SaleResponse sale,
        CancellationToken cancellationToken = default);
}
