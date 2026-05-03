using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Party;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Party;

public class CreatePartyHandlerTests : HandlerTestBase
{
    private CreatePartyHandler BuildHandler() => new(Db, BranchProvider);

    private static CreatePartyCommand CommandFor(PartyType type, string title = "Acme") =>
        new(
            Title: title,
            FirstName: null,
            LastName: null,
            Email: null,
            Phone: null,
            Address: null,
            ContactPerson: null,
            LoyaltyCode: null,
            NationalId: null,
            VatNumber: null,
            TaxId: null,
            Notes: null,
            PartyType: type);

    [Fact]
    public async Task Happy_path_creates_customer_party_and_returns_guid()
    {
        var sut = BuildHandler();
        var cmd = CommandFor(PartyType.Customer, "Customer A");

        var id = await sut.Handle(cmd, CancellationToken.None);

        id.Should().NotBe(Guid.Empty);

        var fresh = await Db.Parties.AsNoTracking().FirstAsync(p => p.Id == id);
        fresh.PartyType.Should().Be(PartyType.Customer);
        fresh.Title.Should().Be("Customer A");
        fresh.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Happy_path_creates_supplier_party()
    {
        var sut = BuildHandler();
        var cmd = CommandFor(PartyType.Supplier, "Supplier A");

        var id = await sut.Handle(cmd, CancellationToken.None);

        var fresh = await Db.Parties.AsNoTracking().FirstAsync(p => p.Id == id);
        fresh.PartyType.Should().Be(PartyType.Supplier);
        fresh.Title.Should().Be("Supplier A");
    }

    [Fact]
    public async Task Branch_scoping_sets_BranchId_to_current_branch()
    {
        var sut = BuildHandler();
        var cmd = CommandFor(PartyType.Customer);

        var id = await sut.Handle(cmd, CancellationToken.None);

        var fresh = await Db.Parties.AsNoTracking().FirstAsync(p => p.Id == id);
        fresh.BranchId.Should().Be(BranchId);
    }
}

