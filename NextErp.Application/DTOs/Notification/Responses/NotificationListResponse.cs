namespace NextErp.Application.DTOs.Notification;

public sealed record NotificationListResponse
{
    public IList<NotificationResponse> Items { get; init; } = new List<NotificationResponse>();
    public int UnreadCount { get; init; }
    public int Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
