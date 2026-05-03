using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Variation;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Variation;

public class UnassignVariationOptionFromProductHandlerTests : HandlerTestBase
{
    private UnassignVariationOptionFromProductHandler BuildHandler() => new(Db);

    private const int ProductId = 100;
    private const int OptionId = 200;

    private async Task SeedAssignmentAsync()
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
        Db.ProductVariationOptions.Add(new ProductVariationOption
        {
            Id = 300,
            ProductId = ProductId,
            VariationOptionId = OptionId,
            Title = "Color",
            DisplayOrder = 0,
            CreatedAt = DateTime.UtcNow,
        });
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Happy_path_removes_ProductVariationOption_row()
    {
        await SeedAssignmentAsync();
        var sut = BuildHandler();

        await sut.Handle(
            new UnassignVariationOptionFromProductCommand(ProductId, OptionId),
            CancellationToken.None);

        var exists = await Db.ProductVariationOptions.AsNoTracking()
            .AnyAsync(p => p.ProductId == ProductId && p.VariationOptionId == OptionId);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Assignment_not_found_throws_not_assigned()
    {
        var sut = BuildHandler();

        var act = async () => await sut.Handle(
            new UnassignVariationOptionFromProductCommand(ProductId: 9999, VariationOptionId: 8888),
            CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*not assigned*");
    }
}

