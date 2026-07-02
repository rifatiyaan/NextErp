using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs.Accounting;

public sealed record UpdateAccountRequest
{
    public string Name { get; init; } = null!;
    public AccountType Type { get; init; }
    public Guid? ParentAccountId { get; init; }
    public bool IsPostingAllowed { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}
