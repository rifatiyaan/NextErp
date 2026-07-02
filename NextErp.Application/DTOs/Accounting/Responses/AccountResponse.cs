using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs.Accounting;

public sealed record AccountResponse
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public AccountType Type { get; init; }
    public string TypeName => Type.ToString();
    public Guid? ParentAccountId { get; init; }
    public string? ParentName { get; init; }
    public bool IsPostingAllowed { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
