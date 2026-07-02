using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs.Accounting;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.Accounting;

namespace NextErp.Application.Handlers.QueryHandlers.Accounting;

public sealed class GetAccountByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetAccountByIdQuery, AccountResponse?>
{
    public async Task<AccountResponse?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken = default)
    {
        return await db.Accounts
            .AsNoTracking()
            .Where(a => a.Id == request.Id)
            .Select(a => new AccountResponse
            {
                Id = a.Id,
                Code = a.Code,
                Name = a.Name,
                Type = a.Type,
                ParentAccountId = a.ParentAccountId,
                ParentName = a.ParentAccount != null ? a.ParentAccount.Name : null,
                IsPostingAllowed = a.IsPostingAllowed,
                Description = a.Description,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public sealed class GetPagedAccountsHandler(IApplicationDbContext db)
    : IRequestHandler<GetPagedAccountsQuery, PagedAccountResponse>
{
    public async Task<PagedAccountResponse> Handle(GetPagedAccountsQuery request, CancellationToken cancellationToken = default)
    {
        var page = request.PageIndex < 1 ? 1 : request.PageIndex;
        var pageSize = request.PageSize < 1 ? 50 : request.PageSize;

        var baseQuery = db.Accounts.AsNoTracking();
        var total = await baseQuery.CountAsync(cancellationToken);

        var filtered = baseQuery;
        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var s = request.SearchText.Trim();
            filtered = filtered.Where(a => a.Code.Contains(s) || a.Name.Contains(s));
        }
        if (request.Type.HasValue)
            filtered = filtered.Where(a => a.Type == request.Type.Value);
        if (request.OnlyPostable == true)
            filtered = filtered.Where(a => a.IsPostingAllowed && a.IsActive);

        var totalDisplay = await filtered.CountAsync(cancellationToken);

        var rows = await filtered
            .OrderBy(a => a.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AccountResponse
            {
                Id = a.Id,
                Code = a.Code,
                Name = a.Name,
                Type = a.Type,
                ParentAccountId = a.ParentAccountId,
                ParentName = a.ParentAccount != null ? a.ParentAccount.Name : null,
                IsPostingAllowed = a.IsPostingAllowed,
                Description = a.Description,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return new PagedAccountResponse
        {
            Total = total,
            TotalDisplay = totalDisplay,
            Data = rows,
        };
    }
}

public sealed class GetJournalEntryByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetJournalEntryByIdQuery, JournalResponse?>
{
    public async Task<JournalResponse?> Handle(GetJournalEntryByIdQuery request, CancellationToken cancellationToken = default)
    {
        return await db.JournalEntries
            .AsNoTracking()
            .Where(e => e.Id == request.Id)
            .Select(e => new JournalResponse
            {
                Id = e.Id,
                EntryNumber = e.EntryNumber,
                EntryDate = e.EntryDate,
                Description = e.Description,
                Status = e.Status,
                ReferenceType = e.ReferenceType,
                ReferenceId = e.ReferenceId,
                Reference = e.Reference,
                CreatedAt = e.CreatedAt,
                TotalDebit = e.Lines.Sum(l => l.Debit),
                TotalCredit = e.Lines.Sum(l => l.Credit),
                Lines = e.Lines
                    .OrderBy(l => l.LineOrder)
                    .Select(l => new JournalLineResponse
                    {
                        Id = l.Id,
                        AccountId = l.AccountId,
                        AccountCode = l.Account.Code,
                        AccountName = l.Account.Name,
                        Description = l.Description,
                        Debit = l.Debit,
                        Credit = l.Credit,
                        LineOrder = l.LineOrder,
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public sealed class GetPagedJournalEntriesHandler(IApplicationDbContext db)
    : IRequestHandler<GetPagedJournalEntriesQuery, PagedJournalResponse>
{
    public async Task<PagedJournalResponse> Handle(GetPagedJournalEntriesQuery request, CancellationToken cancellationToken = default)
    {
        var page = request.PageIndex < 1 ? 1 : request.PageIndex;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var baseQuery = db.JournalEntries.AsNoTracking().Where(e => e.IsActive);
        var total = await baseQuery.CountAsync(cancellationToken);

        var filtered = baseQuery;
        if (request.ReferenceType.HasValue)
            filtered = filtered.Where(e => e.ReferenceType == request.ReferenceType.Value);
        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var s = request.SearchText.Trim();
            filtered = filtered.Where(e =>
                e.EntryNumber.Contains(s) ||
                e.Description.Contains(s) ||
                (e.Reference != null && e.Reference.Contains(s)));
        }

        var totalDisplay = await filtered.CountAsync(cancellationToken);

        // We materialise to a temporary projection that includes the two
        // "headline" lines so the From/To columns can be filled in on the
        // list page without a second query per row.
        var rows = await filtered
            .OrderByDescending(e => e.EntryDate)
            .ThenByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new JournalListRowResponse
            {
                Id = e.Id,
                EntryNumber = e.EntryNumber,
                EntryDate = e.EntryDate,
                Description = e.Description,
                ReferenceType = e.ReferenceType,
                Reference = e.Reference,
                TotalAmount = e.Lines.Sum(l => l.Debit),
                LineCount = e.Lines.Count,
                FromAccount = e.Lines
                    .Where(l => l.Credit > 0)
                    .OrderBy(l => l.LineOrder)
                    .Select(l => l.Account.Code + " – " + l.Account.Name)
                    .FirstOrDefault(),
                ToAccount = e.Lines
                    .Where(l => l.Debit > 0)
                    .OrderBy(l => l.LineOrder)
                    .Select(l => l.Account.Code + " – " + l.Account.Name)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        return new PagedJournalResponse
        {
            Total = total,
            TotalDisplay = totalDisplay,
            Data = rows,
        };
    }
}
