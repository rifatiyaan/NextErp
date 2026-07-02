using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs.Accounting;

public sealed record CreateAccountRequest
{
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public AccountType Type { get; init; }
    public Guid? ParentAccountId { get; init; }
    public bool IsPostingAllowed { get; init; } = true;
    public string? Description { get; init; }
}
