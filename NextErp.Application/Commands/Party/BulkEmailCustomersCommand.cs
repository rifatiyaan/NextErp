using MediatR;

namespace NextErp.Application.Commands;

/// <summary>
/// Enqueue one outbound email per customer id. The handler does the fan-out
/// (one Hangfire job per recipient) so a single bad address can't take down
/// the whole batch — Hangfire's retry policy still kicks in per-job. Returns
/// a small summary describing what got queued vs. skipped (no email on file).
/// </summary>
/// <param name="CustomerIds">Party ids of customers to email. Empty list returns a zero summary.</param>
/// <param name="Subject">Subject line. Required, non-empty.</param>
/// <param name="Body">Plain-text body. HTML is escaped server-side.</param>
public record BulkEmailCustomersCommand(
    IReadOnlyList<Guid> CustomerIds,
    string Subject,
    string Body) : IRequest<BulkEmailCustomersResult>;

/// <summary>
/// Per-request summary returned to the operator. We deliberately separate
/// "queued" from "skipped" so the UI can warn "we couldn't reach 3 of the
/// 50 customers you selected — they have no email on file" without the
/// operator having to dig into Hangfire's dashboard.
///
/// <see cref="QueueableCustomerIds"/> is the resolved subset that has a
/// valid email — the controller iterates this and fires one Hangfire job
/// per id. Returned in the response so the handler stays Hangfire-free.
/// </summary>
public sealed record BulkEmailCustomersResult(
    int Queued,
    int SkippedNoEmail,
    int SkippedNotFound,
    IReadOnlyList<Guid> QueueableCustomerIds);
