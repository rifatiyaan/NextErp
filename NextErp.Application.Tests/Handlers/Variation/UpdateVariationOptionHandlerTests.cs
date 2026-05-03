using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Variation;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Variation;

public class UpdateVariationOptionHandlerTests : HandlerTestBase
{
    private UpdateVariationOptionHandler BuildHandler() => new(Db);

    private async Task<int> SeedOptionAsync(string name = "Original")
    {
        var option = new VariationOption
        {
            Id = 100,
            Name = name,
            Title = name,
            DisplayOrder = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            TenantId = TenantId,
            BranchId = BranchId,
        };
        Db.VariationOptions.Add(option);
        await Db.SaveChangesAsync();
        return option.Id;
    }

    [Fact]
    public async Task Happy_path_updates_name_title_displayOrder_and_stamps_UpdatedAt()
    {
        var id = await SeedOptionAsync("Old");
        var sut = BuildHandler();
        var before = DateTime.UtcNow;

        await sut.Handle(new UpdateVariationOptionCommand(id, "New", DisplayOrder: 7), CancellationToken.None);

        var fresh = await Db.VariationOptions.AsNoTracking().FirstAsync(o => o.Id == id);
        fresh.Name.Should().Be("New");
        fresh.Title.Should().Be("New");
        fresh.DisplayOrder.Should().Be(7);
        fresh.UpdatedAt.Should().NotBeNull();
        fresh.UpdatedAt!.Value.Should().BeOnOrAfter(before).And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Not_found_throws_InvalidOperationException_with_id()
    {
        var sut = BuildHandler();

        var act = async () =>
            await sut.Handle(new UpdateVariationOptionCommand(9999, "X", DisplayOrder: 0), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*9999*");
    }
}

