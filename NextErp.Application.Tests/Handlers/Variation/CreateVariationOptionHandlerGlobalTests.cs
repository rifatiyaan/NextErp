using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Variation;

namespace NextErp.Application.Tests.Handlers.Variation;

public class CreateVariationOptionHandlerGlobalTests : HandlerTestBase
{
    private CreateVariationOptionHandlerGlobal BuildHandler() => new(Db);

    [Fact]
    public async Task Happy_path_creates_active_option_and_returns_id()
    {
        var sut = BuildHandler();
        var cmd = new CreateVariationOptionCommandGlobal("Size", DisplayOrder: 1, TenantId, BranchId: null);

        var id = await sut.Handle(cmd, CancellationToken.None);

        id.Should().BeGreaterThan(0);
        var option = await Db.VariationOptions.AsNoTracking().FirstAsync(o => o.Id == id);
        option.Name.Should().Be("Size");
        option.Title.Should().Be("Size");
        option.DisplayOrder.Should().Be(1);
        option.IsActive.Should().BeTrue();
        option.TenantId.Should().Be(TenantId);
        option.BranchId.Should().BeNull();
    }

    [Fact]
    public async Task Duplicate_name_is_accepted_creating_two_rows()
    {
        // BUG?: handler currently has no uniqueness guard on (TenantId, Name).
        // Flagged: callers may end up with two active "Size" options under the same tenant.
        var sut = BuildHandler();
        var first = await sut.Handle(
            new CreateVariationOptionCommandGlobal("Color", DisplayOrder: 0, TenantId, BranchId: null),
            CancellationToken.None);
        var second = await sut.Handle(
            new CreateVariationOptionCommandGlobal("Color", DisplayOrder: 1, TenantId, BranchId: null),
            CancellationToken.None);

        first.Should().NotBe(second);
        var rows = await Db.VariationOptions.AsNoTracking().Where(o => o.Name == "Color").ToListAsync();
        rows.Should().HaveCount(2);
    }
}

