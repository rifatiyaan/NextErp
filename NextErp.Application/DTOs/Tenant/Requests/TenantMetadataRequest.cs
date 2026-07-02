namespace NextErp.Application.DTOs.Tenant;

public sealed record TenantMetadataRequest
{
    public string? AdminEmail { get; init; }
    public string? SubscriptionPlan { get; init; }
    public DateTime? SubscriptionExpiry { get; init; }
}
