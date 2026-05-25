namespace NextErp.Application.Interfaces;

/// <summary>
/// Sends a free-form email to a single customer. Unlike
/// <see cref="IInvoiceEmailService"/>, this carries no PDF attachment and no
/// sale lookup — it's a thin broadcast primitive used by the bulk-email
/// pipeline. Implementations must be Hangfire-safe (don't hold scoped state
/// across awaits; throw on transient SMTP errors so the retry policy fires).
/// </summary>
public interface ICustomerBroadcastEmailService
{
    /// <summary>
    /// Resolves the customer's email address, renders the message body, and
    /// sends. The handler enqueues one of these per customer id so a partial
    /// failure (one bad recipient) doesn't take down the whole batch.
    /// </summary>
    Task SendBroadcastAsync(
        Guid customerId,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}
