namespace NextErp.Application.DTOs.Branch;

public abstract record BranchRequestBase
{
    public string Name { get; init; } = null!;
    public string? Address { get; init; }
    public BranchMetadataRequest Metadata { get; init; } = new();
}
