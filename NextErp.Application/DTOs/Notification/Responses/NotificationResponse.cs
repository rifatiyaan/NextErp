namespace NextErp.Application.DTOs.Notification;

public sealed record NotificationResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string Type { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string? RelatedEntityType { get; init; }
    public string? RelatedEntityId { get; init; }
    public DateTime? ReadAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
