using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Accounting;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Accounting;

/// <summary>
/// Chart-of-accounts CRUD. Accounts are tenant-scoped (not branch-scoped) —
/// the same CoA is shared across all branches so the trial balance rolls up
/// consistently regardless of where a transaction was posted.
/// </summary>
public sealed class CreateAccountHandler(
    IApplicationDbContext db,
    IUserContext userContext)
    : IRequestHandler<CreateAccountCommand, Guid>
{
    public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken = default)
    {
        var dto = request.Request;
        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new InvalidOperationException("Account code is required.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Account name is required.");

        // Reject duplicate codes — the CoA tree depends on the code being a
        // stable, unique identifier (used in journal exports and reports).
        var codeExists = await db.Accounts
            .AnyAsync(a => a.Code == dto.Code.Trim() && a.IsActive, cancellationToken);
        if (codeExists)
            throw new InvalidOperationException($"An active account with code '{dto.Code}' already exists.");

        Guid tenantId;
        if (dto.ParentAccountId.HasValue)
        {
            var parent = await db.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == dto.ParentAccountId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Parent account not found.");
            tenantId = parent.TenantId;
        }
        else
        {
            // No parent → inherit tenant from any existing account, or fall
            // back to the calling user's tenant. We default to zero-Guid for
            // dev tenants since the app uses 00000000-0000-0000-0000-000000000000
            // as the implicit single-tenant id.
            tenantId = await db.Accounts
                .AsNoTracking()
                .Select(a => a.TenantId)
                .FirstOrDefaultAsync(cancellationToken);
            if (tenantId == Guid.Empty)
            {
                tenantId = userContext.UserId is not null
                    ? Guid.Empty // pet-project: single-tenant
                    : Guid.Empty;
            }
        }

        var account = new Account
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            Type = dto.Type,
            ParentAccountId = dto.ParentAccountId,
            IsPostingAllowed = dto.IsPostingAllowed,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        db.Accounts.Add(account);
        await db.SaveChangesAsync(cancellationToken);
        return account.Id;
    }
}

public sealed class UpdateAccountHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateAccountCommand, bool>
{
    public async Task<bool> Handle(UpdateAccountCommand request, CancellationToken cancellationToken = default)
    {
        var account = await db.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (account == null) return false;

        var dto = request.Request;
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Account name is required.");

        // Reject cycles: an account can't become its own ancestor's parent.
        if (dto.ParentAccountId.HasValue)
        {
            if (dto.ParentAccountId.Value == request.Id)
                throw new InvalidOperationException("An account cannot be its own parent.");
            var ancestor = await db.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == dto.ParentAccountId.Value, cancellationToken);
            while (ancestor != null)
            {
                if (ancestor.Id == request.Id)
                    throw new InvalidOperationException("Cycle detected in account hierarchy.");
                ancestor = ancestor.ParentAccountId.HasValue
                    ? await db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == ancestor.ParentAccountId.Value, cancellationToken)
                    : null;
            }
        }

        account.Name = dto.Name.Trim();
        account.Type = dto.Type;
        account.ParentAccountId = dto.ParentAccountId;
        account.IsPostingAllowed = dto.IsPostingAllowed;
        account.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        account.IsActive = dto.IsActive;
        account.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed class DeactivateAccountHandler(IApplicationDbContext db)
    : IRequestHandler<DeactivateAccountCommand, bool>
{
    public async Task<bool> Handle(DeactivateAccountCommand request, CancellationToken cancellationToken = default)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (account == null) return false;

        // Soft-delete: keep posted journal lines intact for historical reports.
        account.IsActive = false;
        account.IsPostingAllowed = false;
        account.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
