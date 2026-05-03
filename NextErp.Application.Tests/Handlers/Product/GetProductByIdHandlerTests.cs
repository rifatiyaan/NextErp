using AutoMapper;
using NextErp.Application.Handlers.QueryHandlers.Product;
using NextErp.Application.Queries;

namespace NextErp.Application.Tests.Handlers.Product;

public class GetProductByIdHandlerTests : HandlerTestBase
{
    private static readonly IMapper Mapper = BuildMapper();

    private static IMapper BuildMapper()
    {
        var cfg = new MapperConfiguration(c =>
            c.AddMaps(typeof(NextErp.Application.ApplicationAssemblyMarker).Assembly));
        return cfg.CreateMapper();
    }

    private GetProductByIdHandler BuildHandler() => new(Db, BranchProvider, Mapper);

    [Fact]
    public async Task Existing_product_returned_with_id_title_code()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder()
            .WithId(900).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(900).WithTitle("Hammer").WithCode("HM-1").WithCategory(900)
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();

        var sut = BuildHandler();
        var result = await sut.Handle(new GetProductByIdQuery(900), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(900);
        result.Title.Should().Be("Hammer");
        result.Code.Should().Be("HM-1");
    }
}

