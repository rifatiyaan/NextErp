using MediatR;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Handlers.CommandHandlers.Ecommerce;
using NextErp.Application.Handlers.QueryHandlers.Ecommerce;
using NextErp.Application.Queries.Ecommerce;
using NextErp.Application.Tests.Builders;
using NextErp.Application.Tests.Infrastructure;

namespace NextErp.Application.Tests.Handlers.Ecommerce;

public class PublicationHandlersTests : HandlerTestBase
{
    private const int CatA = 600;
    private const int CatB = 601;

    private async Task SeedAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder().WithId(CatA).WithTitle("A").WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Categories.Add(new CategoryBuilder().WithId(CatB).WithTitle("B").WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder().WithId(6000).WithTitle("P1").WithCode("P600001").WithCategory(CatA).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder().WithId(6001).WithTitle("P2").WithCode("P600002").WithCategory(CatB).WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Tree_returns_categories_with_products_and_flags()
    {
        await SeedAsync();
        var sut = new GetEcommercePublicationHandler(Db);

        var tree = await sut.Handle(new GetEcommercePublicationQuery(), CancellationToken.None);

        tree.Should().HaveCount(2);
        tree.Single(c => c.Id == CatA).Products.Should().ContainSingle(p => p.Code == "P600001" && !p.IsPublishedOnline);
    }

    [Fact]
    public async Task Bulk_update_sets_and_clears_flags()
    {
        await SeedAsync();
        var sut = new SetEcommercePublicationHandler(Db);

        await sut.Handle(new SetEcommercePublicationCommand(
            PublishCategoryIds: new() { CatA },
            UnpublishCategoryIds: new(),
            PublishProductIds: new() { 6000 },
            UnpublishProductIds: new()), CancellationToken.None);

        (await Db.Categories.AsNoTracking().FirstAsync(c => c.Id == CatA)).IsPublishedOnline.Should().BeTrue();
        (await Db.Products.AsNoTracking().FirstAsync(p => p.Id == 6000)).IsPublishedOnline.Should().BeTrue();
        (await Db.Products.AsNoTracking().FirstAsync(p => p.Id == 6001)).IsPublishedOnline.Should().BeFalse();

        await sut.Handle(new SetEcommercePublicationCommand(
            new(), new() { CatA }, new(), new() { 6000 }), CancellationToken.None);
        (await Db.Categories.AsNoTracking().FirstAsync(c => c.Id == CatA)).IsPublishedOnline.Should().BeFalse();
        (await Db.Products.AsNoTracking().FirstAsync(p => p.Id == 6000)).IsPublishedOnline.Should().BeFalse();
    }
}
