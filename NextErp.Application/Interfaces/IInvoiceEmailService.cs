using NextErp.Application.DTOs;

namespace NextErp.Application.Interfaces;

/// <summary>
/// Sends an invoice email with the rendered PDF attached. Implementations
/// should be safe to call from a Hangfire background job — that means: don't
/// hold onto request-scoped state across awaits, throw on transient SMTP
/// errors so Hangfire's retry policy can re-run the job, and accept all
/// arguments in JSON-serialisable shapes (Guid + record types).
/// </summary>
public interface IInvoiceEmailService
{
    /// <summary>
    /// Fetches the sale, generates the PDF, and dispatches the email via
    /// configured SMTP. Designed to be invoked indirectly via
    /// <c>BackgroundJob.Enqueue&lt;IInvoiceEmailService&gt;(...)</c>.
    /// </summary>
    Task SendSaleInvoiceAsync(
        Guid saleId,
        EmailMessageRequest message,
        CancellationToken cancellationToken = default);
}
