using NextErp.Application.Handlers.QueryHandlers.Product;
using NextErp.Application.Queries;

namespace NextErp.Application.Tests.Handlers.Product;

public class GetNextProductCodeHandlerTests : HandlerTestBase
{
    private GetNextProductCodeHandler BuildHandler() => new(Db, BranchProvider);

    private async Task SeedBranchAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Returns_first_code_when_no_products_exist()
    {
        await SeedBranchAsync();
        var sut = BuildHandler();

        var code = await sut.Handle(new GetNextProductCodeQuery(), CancellationToken.None);

        code.Should().Be("P000001");
    }

    [Fact]
    public async Task Returns_next_code_after_existing_max()
    {
        await SeedBranchAsync();
        Db.Products.Add(new ProductBuilder()
            .WithCode("P000005").WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
        var sut = BuildHandler();

        var code = await sut.Handle(new GetNextProductCodeQuery(), CancellationToken.None);

        code.Should().Be("P000006");
    }
}
