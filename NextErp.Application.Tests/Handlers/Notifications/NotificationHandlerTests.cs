using NextErp.Application.Commands.Notifications;
using NextErp.Application.Handlers.CommandHandlers.Notifications;
using NextErp.Application.Handlers.QueryHandlers.Notifications;
using NextErp.Application.Queries.Notifications;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Notifications;

public class NotificationHandlerTests : HandlerTestBase
{
    private Notification SeedNotification(
        Guid? userId = null,
        DateTime? createdAt = null,
        DateTime? readAt = null,
        Guid? branchId = null,
        string type = "ProductCreated",
        string title = "New product",
        string message = "Test message")
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Title = title,
            TenantId = TenantId,
            BranchId = branchId ?? BranchId,
            UserId = userId,
            Type = type,
            Message = message,
            ReadAt = readAt,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            IsActive = true,
        };
        Db.Notifications.Add(notification);
        return notification;
    }

    [Fact]
    public async Task GetNotifications_returns_paged_ordered_DESC_by_CreatedAt()
    {
        SeedNotification(createdAt: DateTime.UtcNow.AddMinutes(-30), title: "Older");
        SeedNotification(createdAt: DateTime.UtcNow.AddMinutes(-5), title: "Newer");
        SeedNotification(createdAt: DateTime.UtcNow.AddMinutes(-15), title: "Middle");
        await Db.SaveChangesAsync();

        var sut = new GetNotificationsHandler(Db, UserContext);

        var result = await sut.Handle(new GetNotificationsQuery(Page: 1, PageSize: 10), CancellationToken.None);

        result.Items.Should().HaveCount(3);
        result.Items[0].Title.Should().Be("Newer");
        result.Items[1].Title.Should().Be("Middle");
        result.Items[2].Title.Should().Be("Older");
        result.Total.Should().Be(3);
    }

    [Fact]
    public async Task GetNotifications_UnreadOnly_filters_out_read_rows()
    {
        SeedNotification(readAt: DateTime.UtcNow, title: "Already read");
        SeedNotification(readAt: null, title: "Still unread");
        SeedNotification(readAt: null, title: "Another unread");
        await Db.SaveChangesAsync();

        var sut = new GetNotificationsHandler(Db, UserContext);

        var result = await sut.Handle(new GetNotificationsQuery(UnreadOnly: true), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(n => n.ReadAt == null);
        result.UnreadCount.Should().Be(2);
        result.Total.Should().Be(2);
    }

    [Fact]
    public async Task GetNotifications_is_branch_scoped_via_global_filter()
    {
        SeedNotification(title: "Mine");
        SeedNotification(title: "OtherBranch", branchId: Guid.NewGuid());
        await Db.SaveChangesAsync();

        var sut = new GetNotificationsHandler(Db, UserContext);

        var result = await sut.Handle(new GetNotificationsQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Mine");
    }

    [Fact]
    public async Task GetNotifications_returns_user_targeted_and_broadcast_only()
    {
        SeedNotification(userId: UserId, title: "ForMe");
        SeedNotification(userId: null, title: "Broadcast");
        SeedNotification(userId: Guid.NewGuid(), title: "ForSomeoneElse");
        await Db.SaveChangesAsync();

        var sut = new GetNotificationsHandler(Db, UserContext);

        var result = await sut.Handle(new GetNotificationsQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Select(i => i.Title).Should().BeEquivalentTo(new[] { "ForMe", "Broadcast" });
    }

    [Fact]
    public async Task MarkNotificationRead_sets_ReadAt_when_unread()
    {
        var n = SeedNotification(userId: UserId);
        await Db.SaveChangesAsync();

        var sut = new MarkNotificationReadHandler(Db, UserContext);

        await sut.Handle(new MarkNotificationReadCommand(n.Id), CancellationToken.None);

        var stored = await Db.Notifications.AsNoTracking().FirstAsync(x => x.Id == n.Id);
        stored.ReadAt.Should().NotBeNull();
        stored.ReadAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task MarkNotificationRead_does_not_overwrite_ReadAt_when_already_read()
    {
        var earlier = DateTime.UtcNow.AddMinutes(-10);
        var n = SeedNotification(userId: UserId, readAt: earlier);
        await Db.SaveChangesAsync();

        var sut = new MarkNotificationReadHandler(Db, UserContext);

        await sut.Handle(new MarkNotificationReadCommand(n.Id), CancellationToken.None);

        var stored = await Db.Notifications.AsNoTracking().FirstAsync(x => x.Id == n.Id);
        stored.ReadAt!.Value.Should().BeCloseTo(earlier, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task MarkAllNotificationsRead_clears_unread_for_current_user()
    {
        SeedNotification(userId: UserId, title: "MineUnread1");
        SeedNotification(userId: UserId, title: "MineUnread2");
        SeedNotification(userId: null, title: "BroadcastUnread");
        SeedNotification(userId: UserId, readAt: DateTime.UtcNow.AddMinutes(-10), title: "MineAlreadyRead");
        SeedNotification(userId: Guid.NewGuid(), title: "ForSomeoneElseUnread");
        await Db.SaveChangesAsync();

        var sut = new MarkAllNotificationsReadHandler(Db, UserContext);

        await sut.Handle(new MarkAllNotificationsReadCommand(), CancellationToken.None);

        var rows = await Db.Notifications.AsNoTracking().ToListAsync();
        // Mine + Broadcast should be marked read; the foreign-user one stays untouched.
        rows.Where(r => r.Title.StartsWith("Mine") || r.Title == "BroadcastUnread")
            .Should().OnlyContain(r => r.ReadAt != null);
        rows.First(r => r.Title == "ForSomeoneElseUnread").ReadAt.Should().BeNull();
    }

    [Fact]
    public async Task NotificationService_RecordAsync_stages_a_row_without_saving()
    {
        // Caller's SaveChanges flushes — RecordAsync should not save by itself.
        await Notifications.RecordAsync(
            type: "ProductCreated",
            title: "Test",
            message: "Hello");

        var beforeSave = Db.Notifications.Local.Count;
        beforeSave.Should().Be(1);

        var persistedBeforeSave = await Db.Notifications.AsNoTracking().CountAsync();
        persistedBeforeSave.Should().Be(0);

        await Db.SaveChangesAsync();

        var persistedAfter = await Db.Notifications.AsNoTracking().CountAsync();
        persistedAfter.Should().Be(1);
    }
}
