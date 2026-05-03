using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Variation;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Variation;

public class CreateVariationValueHandlerTests : HandlerTestBase
{
    private CreateVariationValueHandler BuildHandler() => new(Db);

    private async Task<int> SeedOptionAsync(string name = "Color")
    {
        var option = new VariationOption
        {
            Id = 100,
            Name = name,
            Title = name,
            DisplayOrder = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            TenantId = TenantId,
            BranchId = BranchId,
        };
        Db.VariationOptions.Add(option);
        await Db.SaveChangesAsync();
        return option.Id;
    }

    [Fact]
    public async Task Happy_path_creates_value_inheriting_tenant_and_branch_from_option()
    {
        var optionId = await SeedOptionAsync();
        var sut = BuildHandler();

        var id = await sut.Handle(
            new CreateVariationValueCommand(optionId, "Red", DisplayOrder: 3),
            CancellationToken.None);

        id.Should().BeGreaterThan(0);
        var fresh = await Db.VariationValues.AsNoTracking().FirstAsync(v => v.Id == id);
        fresh.Value.Should().Be("Red");
        fresh.Name.Should().Be("Red");
        fresh.Title.Should().Be("Red");
        fresh.DisplayOrder.Should().Be(3);
        fresh.VariationOptionId.Should().Be(optionId);
        fresh.IsActive.Should().BeTrue();
        fresh.TenantId.Should().Be(TenantId);
        fresh.BranchId.Should().Be(BranchId);
    }

    [Fact]
    public async Task Parent_option_not_found_throws_InvalidOperationException_with_id()
    {
        var sut = BuildHandler();

        var act = async () =>
            await sut.Handle(new CreateVariationValueCommand(9999, "X", DisplayOrder: 0), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*9999*");
    }
}

