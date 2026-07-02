using NextErp.Application.Tests.Builders;
using NextErp.Application.Tests.Infrastructure;

namespace NextErp.Application.Tests.Handlers.Ecommerce;

public class PublicationFlagTests : HandlerTestBase
{
    [Fact]
    public async Task Products_and_categories_default_to_unpublished()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder()
            .WithId(500).WithTitle("Cat").WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithCode("P900001").WithCategory(500).WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();

        (await Db.Products.AsNoTracking().FirstAsync()).IsPublishedOnline.Should().BeFalse();
        (await Db.Categories.AsNoTracking().FirstAsync()).IsPublishedOnline.Should().BeFalse();
    }
}
