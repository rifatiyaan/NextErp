using NextErp.Application.Handlers.QueryHandlers.Category;
using NextErp.Application.Queries;

namespace NextErp.Application.Tests.Handlers.Category;

public class GetCategoryByIdHandlerTests : HandlerTestBase
{
    private GetCategoryByIdHandler BuildHandler() => new(Db);

    [Fact]
    public async Task Existing_active_category_is_returned()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder()
            .WithId(800).WithTitle("Tools").WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();

        var sut = BuildHandler();
        var result = await sut.Handle(new GetCategoryByIdQuery(800), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(800);
        result.Title.Should().Be("Tools");
    }
}

