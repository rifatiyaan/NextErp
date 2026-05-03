using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Variation;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Variation;

public class AssignVariationOptionToProductHandlerTests : HandlerTestBase
{
    private AssignVariationOptionToProductHandler BuildHandler() => new(Db);

    private const int ProductId = 100;
    private const int OptionId = 200;

    private async Task SeedAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder().WithId(100).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(ProductId).WithCategory(100).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.VariationOptions.Add(new VariationOption
        {
            Id = OptionId,
            Name = "Color",
            Title = "Color",
            CreatedAt = DateTime.UtcNow,
            TenantId = TenantId,
            BranchId = BranchId,
        });
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Happy_path_creates_ProductVariationOption_with_option_name_as_title()
    {
        await SeedAsync();
        var sut = BuildHandler();

        var id = await sut.Handle(
            new AssignVariationOptionToProductCommand(ProductId, OptionId, DisplayOrder: 2),
            CancellationToken.None);

        id.Should().BeGreaterThan(0);
        var pvo = await Db.ProductVariationOptions.AsNoTracking().FirstAsync(p => p.Id == id);
        pvo.ProductId.Should().Be(ProductId);
        pvo.VariationOptionId.Should().Be(OptionId);
        pvo.DisplayOrder.Should().Be(2);
        pvo.Title.Should().Be("Color");
    }

    [Fact]
    public async Task Re_assigning_same_pair_throws_already_assigned()
    {
        await SeedAsync();
        var sut = BuildHandler();
        await sut.Handle(
            new AssignVariationOptionToProductCommand(ProductId, OptionId, DisplayOrder: 0),
            CancellationToken.None);

        var act = async () => await sut.Handle(
            new AssignVariationOptionToProductCommand(ProductId, OptionId, DisplayOrder: 1),
            CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*already assigned*");

        // Still exactly one row.
        var rows = await Db.ProductVariationOptions.AsNoTracking()
            .Where(p => p.ProductId == ProductId && p.VariationOptionId == OptionId)
            .ToListAsync();
        rows.Should().HaveCount(1);
    }
}

