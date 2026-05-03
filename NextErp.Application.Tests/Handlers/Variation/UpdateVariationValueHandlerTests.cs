using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Variation;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Variation;

public class UpdateVariationValueHandlerTests : HandlerTestBase
{
    private UpdateVariationValueHandler BuildHandler() => new(Db);

    private async Task<int> SeedValueAsync(string value = "Original")
    {
        var option = new VariationOption
        {
            Id = 100,
            Name = "Color",
            Title = "Color",
            CreatedAt = DateTime.UtcNow,
            TenantId = TenantId,
            BranchId = BranchId,
        };
        var v = new VariationValue
        {
            Id = 200,
            Value = value,
            Name = value,
            Title = value,
            DisplayOrder = 0,
            VariationOptionId = option.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            TenantId = TenantId,
            BranchId = BranchId,
        };
        Db.VariationOptions.Add(option);
        Db.VariationValues.Add(v);
        await Db.SaveChangesAsync();
        return v.Id;
    }

    [Fact]
    public async Task Happy_path_updates_value_name_title_displayOrder_and_stamps_UpdatedAt()
    {
        var id = await SeedValueAsync("Old");
        var sut = BuildHandler();
        var before = DateTime.UtcNow;

        await sut.Handle(new UpdateVariationValueCommand(id, "Blue", DisplayOrder: 4), CancellationToken.None);

        var fresh = await Db.VariationValues.AsNoTracking().FirstAsync(v => v.Id == id);
        fresh.Value.Should().Be("Blue");
        fresh.Name.Should().Be("Blue");
        fresh.Title.Should().Be("Blue");
        fresh.DisplayOrder.Should().Be(4);
        fresh.UpdatedAt.Should().NotBeNull();
        fresh.UpdatedAt!.Value.Should().BeOnOrAfter(before).And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Not_found_throws_InvalidOperationException_with_id()
    {
        var sut = BuildHandler();

        var act = async () =>
            await sut.Handle(new UpdateVariationValueCommand(9999, "X", DisplayOrder: 0), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*9999*");
    }
}

