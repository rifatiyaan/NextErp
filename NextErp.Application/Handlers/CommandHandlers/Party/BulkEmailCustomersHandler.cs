using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Party;

/// <summary>
/// Partition step for the bulk-email batch action on the Customers page.
///
/// The handler does NOT enqueue Hangfire jobs itself — Application doesn't
/// reference Hangfire (and shouldn't, that's an infrastructure concern). It
/// classifies the requested ids into "has email" vs. "no email" vs. "not
/// found" and returns the `Queue` list back to the controller, which is the
/// one with access to <c>IBackgroundJobClient</c>. The controller then fires
/// one job per recipient.
///
/// This split keeps the handler unit-testable without a Hangfire harness and
/// matches the pattern already used in <c>SaleController.SendInvoiceEmail</c>.
/// </summary>
public sealed class BulkEmailCustomersHandler(IApplicationDbContext db)
    : IRequestHandler<BulkEmailCustomersCommand, BulkEmailCustomersResult>
{
    public async Task<BulkEmailCustomersResult> Handle(
        BulkEmailCustomersCommand request,
        CancellationToken cancellationToken = default)
    {
        if (request.CustomerIds.Count == 0)
            return new BulkEmailCustomersResult(0, 0, 0, Array.Empty<Guid>());

        var subject = request.Subject.Trim();
        var body = request.Body.Trim();
        if (subject.Length == 0)
            throw new ArgumentException("Subject is required.", nameof(request));
        if (body.Length == 0)
            throw new ArgumentException("Body is required.", nameof(request));

        // Pull (id, email) for the requested set in one round-trip. We
        // explicitly filter to PartyType.Customer + IsActive so a stale
        // selection (someone deactivated mid-flow) doesn't get a stray email.
        var requestedIds = request.CustomerIds.Distinct().ToList();
        var found = await db.Parties
            .AsNoTracking()
            .Where(p => requestedIds.Contains(p.Id)
                        && p.PartyType == PartyType.Customer
                        && p.IsActive)
            .Select(p => new { p.Id, p.Email })
            .ToListAsync(cancellationToken);

        var foundIds = found.Select(x => x.Id).ToHashSet();
        var notFound = requestedIds.Count - foundIds.Count;

        var queueable = found
            .Where(x => !string.IsNullOrWhiteSpace(x.Email))
            .Select(x => x.Id)
            .ToList();
        var noEmail = found.Count - queueable.Count;

        return new BulkEmailCustomersResult(
            Queued: queueable.Count,
            SkippedNoEmail: noEmail,
            SkippedNotFound: notFound,
            QueueableCustomerIds: queueable);
    }
}
