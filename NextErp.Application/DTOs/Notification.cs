namespace NextErp.Application.DTOs;

public static class Notification
{
    public static class Response
    {
        public class Item
        {
            public Guid Id { get; set; }
            public string Title { get; set; } = null!;
            public string Type { get; set; } = null!;
            public string Message { get; set; } = null!;
            public string? RelatedEntityType { get; set; }
            public string? RelatedEntityId { get; set; }
            public DateTime? ReadAt { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class List
        {
            public IList<Item> Items { get; set; } = new List<Item>();
            public int UnreadCount { get; set; }
            public int Total { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
        }
    }
}
