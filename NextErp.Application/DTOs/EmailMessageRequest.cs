namespace NextErp.Application.DTOs;

/// <summary>
/// User-supplied parameters for an outbound invoice email. Kept as a record
/// so it serialises cleanly into Hangfire's job arguments table — the worker
/// pulls these back out by deserialising JSON, so anything non-trivial (file
/// streams, services, DbContext) must NOT live here.
/// </summary>
public sealed record EmailMessageRequest
{
    /// <summary>Recipient address. Required.</summary>
    public string To { get; init; } = null!;

    /// <summary>
    /// Optional override of the email subject. The service falls back to
    /// "Invoice {SaleNumber}" when null/empty so the subject line still
    /// makes sense without the user typing one.
    /// </summary>
    public string? Subject { get; init; }

    /// <summary>
    /// Optional plain-text body shown above the auto-generated invoice
    /// summary. Markdown / HTML is NOT rendered; the worker wraps this in
    /// a simple HTML paragraph to keep injection surface small.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>Optional CC list (comma-separated).</summary>
    public string? Cc { get; init; }
}
