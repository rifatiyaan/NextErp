using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Accounting;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Accounting;

/// <summary>
/// Books a balanced double-entry transfer between two accounts.
///
/// Accounting convention: when money moves *into* an asset account (e.g.
/// cash → bank), the destination is debited and the source is credited.
/// We don't try to be clever about sign flips for different AccountType
/// combinations — the user picks the From/To and we always debit-destination
/// / credit-source. That's the conventional bookkeeping rule, and reverse
/// transfers (e.g. drawing from equity) just look like a debit on equity.
///
/// The entry is created in Posted status with ReferenceType=Transfer; the
/// ReferenceId is left null because the entry is its own canonical record
/// (there's no separate "Transfer" entity to point back to).
/// </summary>
public sealed class CreateAccountTransferHandler(
    IApplicationDbContext db,
    IBranchProvider branchProvider,
    IUserContext userContext)
    : IRequestHandler<CreateAccountTransferCommand, Guid>
{
    public async Task<Guid> Handle(CreateAccountTransferCommand request, CancellationToken cancellationToken = default)
    {
        var dto = request.Request;
        if (dto.FromAccountId == Guid.Empty || dto.ToAccountId == Guid.Empty)
            throw new InvalidOperationException("Both source and destination accounts are required.");
        if (dto.FromAccountId == dto.ToAccountId)
            throw new InvalidOperationException("Source and destination accounts must be different.");
        if (dto.Amount <= 0)
            throw new InvalidOperationException("Transfer amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(dto.Description))
            throw new InvalidOperationException("A description is required for audit trail.");

        // Load both accounts in one round-trip and validate before we touch
        // anything. We bypass the soft-delete filter so an explicit "this
        // account is inactive" error is more useful than a generic 'not found'.
        var ids = new[] { dto.FromAccountId, dto.ToAccountId };
        var accounts = await db.Accounts
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(a => ids.Contains(a.Id))
            .ToListAsync(cancellationToken);

        var from = accounts.FirstOrDefault(a => a.Id == dto.FromAccountId)
            ?? throw new InvalidOperationException("Source account not found.");
        var to = accounts.FirstOrDefault(a => a.Id == dto.ToAccountId)
            ?? throw new InvalidOperationException("Destination account not found.");

        if (!from.IsActive || !from.IsPostingAllowed)
            throw new InvalidOperationException($"Account '{from.Code} – {from.Name}' is not open for postings.");
        if (!to.IsActive || !to.IsPostingAllowed)
            throw new InvalidOperationException($"Account '{to.Code} – {to.Name}' is not open for postings.");
        if (from.TenantId != to.TenantId)
            throw new InvalidOperationException("Source and destination accounts belong to different tenants.");

        var branchId = branchProvider.IsGlobal()
            ? branchProvider.GetBranchId() ?? Guid.Empty
            : branchProvider.GetRequiredBranchId();
        if (branchId == Guid.Empty)
        {
            // Fallback: pick the user's first branch, otherwise zero. The
            // BranchScoped filter falls back to "all" for zero, so this still
            // works for system / dev scenarios.
            branchId = Guid.Empty;
        }

        var now = DateTime.UtcNow;
        var entryNumber = GenerateEntryNumber(now);
        var amount = decimal.Round(dto.Amount, 2, MidpointRounding.AwayFromZero);

        var entry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            TenantId = from.TenantId,
            BranchId = branchId,
            EntryNumber = entryNumber,
            EntryDate = dto.EntryDate ?? now,
            Description = dto.Description.Trim(),
            Status = JournalEntryStatus.Posted,
            ReferenceType = JournalEntryReferenceType.Transfer,
            ReferenceId = null,
            Reference = string.IsNullOrWhiteSpace(dto.Reference) ? null : dto.Reference.Trim(),
            CreatedById = userContext.UserId ?? Guid.Empty,
            CreatedAt = now,
            IsActive = true,
        };

        // Debit the destination — money flows INTO this account.
        entry.Lines.Add(new JournalLine
        {
            Id = Guid.NewGuid(),
            JournalEntryId = entry.Id,
            AccountId = to.Id,
            Description = $"Transfer in from {from.Code} – {from.Name}",
            Debit = amount,
            Credit = 0m,
            LineOrder = 1,
            CreatedAt = now,
            IsActive = true,
        });
        // Credit the source — money flows OUT of this account.
        entry.Lines.Add(new JournalLine
        {
            Id = Guid.NewGuid(),
            JournalEntryId = entry.Id,
            AccountId = from.Id,
            Description = $"Transfer out to {to.Code} – {to.Name}",
            Debit = 0m,
            Credit = amount,
            LineOrder = 2,
            CreatedAt = now,
            IsActive = true,
        });

        // Last-ditch invariant check — should always pass given the
        // construction above, but cheap to assert and the entity has the
        // helper for exactly this reason.
        if (!entry.IsBalanced)
            throw new InvalidOperationException("Transfer entry is not balanced. This is a bug.");

        db.JournalEntries.Add(entry);
        await db.SaveChangesAsync(cancellationToken);
        return entry.Id;
    }

    /// <summary>
    /// JE-YYYYMMDD-XXXXXXXX. Same shape as Sale/Purchase numbering so the
    /// audit trail reads consistently.
    /// </summary>
    private static string GenerateEntryNumber(DateTime now)
    {
        var suffix = Guid.NewGuid().ToString()[..8].ToUpperInvariant();
        return $"JE-{now:yyyyMMdd}-{suffix}";
    }
}
