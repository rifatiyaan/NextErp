using NextErp.Application.Handlers.QueryHandlers.Party;
using NextErp.Application.Queries;

namespace NextErp.Application.Tests.Handlers.Party;

public class GetPartyByIdHandlerTests : HandlerTestBase
{
    private GetPartyByIdHandler BuildHandler() => new(Db);

    [Fact]
    public async Task Existing_party_returned()
    {
        var partyId = Guid.NewGuid();
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Parties.Add(new PartyBuilder()
            .WithId(partyId).WithTitle("Acme Co").WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();

        var sut = BuildHandler();
        var result = await sut.Handle(new GetPartyByIdQuery(partyId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(partyId);
        result.Title.Should().Be("Acme Co");
    }
}

