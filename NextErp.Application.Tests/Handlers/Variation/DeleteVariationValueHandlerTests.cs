using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Variation;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Variation;

public class DeleteVariationValueHandlerTests : HandlerTestBase
{
    private DeleteVariationValueHandler BuildHandler() => new(Db);

    private async Task<int> SeedValueAsync()
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
            Value = "Red",
            Name = "Red",
            Title = "Red",
            DisplayOrder = 0,
            VariationOptionId = option.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            TenantId = TenantId,
            BranchId = BranchId,
        };
        Db.VariationOptions.Add(option);
        Db.VariationValues.Add(v);
        await Db.SaveChangesAsync();
        return v.Id;
    }

    [Fact]
    public async Task Happy_path_hard_deletes_row_from_VariationValues()
    {
        var id = await SeedValueAsync();
        var sut = BuildHandler();

        await sut.Handle(new DeleteVariationValueCommand(id), CancellationToken.None);

        var exists = await Db.VariationValues.AsNoTracking().AnyAsync(v => v.Id == id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Not_found_throws_InvalidOperationException_with_id()
    {
        var sut = BuildHandler();

        var act = async () =>
            await sut.Handle(new DeleteVariationValueCommand(9999), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*9999*");
    }
}

