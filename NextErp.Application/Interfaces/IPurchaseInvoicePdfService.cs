using PurchaseDto = NextErp.Application.DTOs.Purchase;

namespace NextErp.Application.Interfaces;

/// <summary>
/// Generates a printable PDF for a single purchase invoice. Mirrors
/// <see cref="IInvoicePdfService"/> for sales — caller passes a hydrated
/// <see cref="PurchaseDto.Response.Get.Single"/> (with line items + metadata)
/// and gets back the rendered bytes.
/// </summary>
public interface IPurchaseInvoicePdfService
{
    Task<byte[]> RenderPurchaseInvoiceAsync(
        PurchaseDto.Response.Get.Single purchase,
        CancellationToken cancellationToken = default);
}
