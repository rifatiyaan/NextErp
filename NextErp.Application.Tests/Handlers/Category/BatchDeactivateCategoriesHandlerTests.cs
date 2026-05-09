using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Category;

namespace NextErp.Application.Tests.Handlers.Category;

public class BatchDeactivateCategoriesHandlerTests : HandlerTestBase
{
    private BatchDeactivateCategoriesHandler BuildHandler() => new(Db);

    private async Task SeedActiveAsync(params int[] ids)
    {
        foreach (var id in ids)
        {
            Db.Categories.Add(new CategoryBuilder()
                .WithId(id).WithTitle($"Cat-{id}")
                .WithTenant(TenantId).WithBranch(BranchId).Build());
        }
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Happy_path_deactivates_all_active_rows_and_sets_UpdatedAt()
    {
        await SeedActiveAsync(101, 102, 103);
        var before = DateTime.UtcNow;
        var sut = BuildHandler();

        var count = await sut.Handle(
            new BatchDeactivateCategoriesCommand(new[] { 101, 102, 103 }),
            CancellationToken.None);

        count.Should().Be(3);
        var rows = await Db.Categories.AsNoTracking()
            .Where(c => new[] { 101, 102, 103 }.Contains(c.Id)).ToListAsync();
        rows.Should().HaveCount(3);
        rows.Should().OnlyContain(c => c.IsActive == false);
        rows.Should().OnlyContain(c =>
            c.UpdatedAt != null
            && c.UpdatedAt!.Value >= before
            && c.UpdatedAt!.Value <= DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public async Task Already_inactive_rows_are_skipped_and_count_reflects_actual_flips()
    {
        await SeedActiveAsync(201, 202, 203);
        // Pre-flip 202 to inactive — handler should ignore it.
        var preInactive = await Db.Categories.FirstAsync(c => c.Id == 202);
        preInactive.IsActive = false;
        preInactive.UpdatedAt = DateTime.UtcNow.AddDays(-1);
        await Db.SaveChangesAsync();

        var preStamp = preInactive.UpdatedAt!.Value;
        var sut = BuildHandler();

        var count = await sut.Handle(
            new BatchDeactivateCategoriesCommand(new[] { 201, 202, 203 }),
            CancellationToken.None);

        count.Should().Be(2);
        // Pre-inactive row's UpdatedAt is NOT touched.
        var fresh = await Db.Categories.AsNoTracking().FirstAsync(c => c.Id == 202);
        fresh.UpdatedAt!.Value.Should().BeCloseTo(preStamp, TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public async Task Empty_id_match_returns_zero_without_writes()
    {
        await SeedActiveAsync(301);
        var sut = BuildHandler();

        // Pass ids that don't exist; nothing should change.
        var count = await sut.Handle(
            new BatchDeactivateCategoriesCommand(new[] { 9001, 9002 }),
            CancellationToken.None);

        count.Should().Be(0);
        var fresh = await Db.Categories.AsNoTracking().FirstAsync(c => c.Id == 301);
        fresh.IsActive.Should().BeTrue();
        fresh.UpdatedAt.Should().BeNull();
    }
}
