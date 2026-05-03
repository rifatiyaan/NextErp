using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Variation;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Variation;

public class DeleteVariationOptionHandlerTests : HandlerTestBase
{
    private DeleteVariationOptionHandler BuildHandler() => new(Db);

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
    public async Task Happy_path_hard_deletes_row_from_VariationOptions()
    {
        var id = await SeedOptionAsync();
        var sut = BuildHandler();

        await sut.Handle(new DeleteVariationOptionCommand(id), CancellationToken.None);

        var exists = await Db.VariationOptions.AsNoTracking().AnyAsync(o => o.Id == id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Not_found_throws_InvalidOperationException_with_id()
    {
        var sut = BuildHandler();

        var act = async () =>
            await sut.Handle(new DeleteVariationOptionCommand(9999), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*9999*");
    }
}

