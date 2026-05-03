using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Party;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Party;

public class UpdatePartyHandlerTests : HandlerTestBase
{
    private UpdatePartyHandler BuildHandler() => new(Db);

    private async Task SeedBranchAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        await Db.SaveChangesAsync();
    }

    private async Task<Guid> SeedPartyAsync(Guid? branchOverride = null, string title = "Original Customer")
    {
        var partyId = Guid.NewGuid();
        Db.Parties.Add(new PartyBuilder()
            .WithId(partyId)
            .WithTitle(title)
            .WithTenant(TenantId)
            .WithBranch(branchOverride ?? BranchId)
            .Build());
        await Db.SaveChangesAsync();
        return partyId;
    }

    [Fact]
    public async Task Happy_path_updates_fields_and_flushes_to_db()
    {
        await SeedBranchAsync();
        var partyId = await SeedPartyAsync();
        var sut = BuildHandler();

        var cmd = new UpdatePartyCommand(
            Id: partyId,
            Title: "Updated Customer",
            FirstName: "Ola",
            LastName: "Nordmann",
            Email: "ola@example.no",
            Phone: "+47 22 22 22 22",
            Address: "Karl Johans gate 1, Oslo",
            ContactPerson: null,
            LoyaltyCode: "L-001",
            NationalId: null,
            VatNumber: null,
            TaxId: null,
            Notes: "Updated note",
            PartyType: PartyType.Customer,
            IsActive: true);

        await sut.Handle(cmd, CancellationToken.None);

        var fresh = await Db.Parties.AsNoTracking().FirstAsync(p => p.Id == partyId);
        fresh.Title.Should().Be("Updated Customer");
        fresh.Email.Should().Be("ola@example.no");
        fresh.Phone.Should().Be("+47 22 22 22 22");
        fresh.Address.Should().Be("Karl Johans gate 1, Oslo");
        fresh.LoyaltyCode.Should().Be("L-001");
        fresh.Notes.Should().Be("Updated note");
        fresh.UpdatedAt.Should().NotBeNull();
        fresh.UpdatedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Not_found_throws_InvalidOperationException_with_id()
    {
        await SeedBranchAsync();
        var sut = BuildHandler();
        var missingId = Guid.NewGuid();

        var cmd = new UpdatePartyCommand(
            Id: missingId,
            Title: "X",
            FirstName: null, LastName: null, Email: null, Phone: null, Address: null,
            ContactPerson: null, LoyaltyCode: null, NationalId: null, VatNumber: null,
            TaxId: null, Notes: null,
            PartyType: PartyType.Customer);

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage($"*{missingId}*");
    }

    [Fact]
    public async Task Branch_scoping_hides_party_from_other_branch()
    {
        // Seed a party that lives in a *different* branch. With non-global BranchProvider
        // (the default), the global query filter strips it from dbContext.Parties — the
        // handler should treat it as not found.
        await SeedBranchAsync();
        var otherBranchId = Guid.NewGuid();
        Db.Branches.Add(new BranchBuilder().WithId(otherBranchId).WithTenant(TenantId).Build());
        await Db.SaveChangesAsync();

        var partyId = await SeedPartyAsync(branchOverride: otherBranchId);
        var sut = BuildHandler();

        var cmd = new UpdatePartyCommand(
            Id: partyId,
            Title: "X",
            FirstName: null, LastName: null, Email: null, Phone: null, Address: null,
            ContactPerson: null, LoyaltyCode: null, NationalId: null, VatNumber: null,
            TaxId: null, Notes: null,
            PartyType: PartyType.Customer);

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task PartyType_change_supplier_to_customer_is_persisted()
    {
        await SeedBranchAsync();
        var partyId = Guid.NewGuid();
        Db.Parties.Add(new PartyBuilder()
            .WithId(partyId).AsSupplier()
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
        var sut = BuildHandler();

        var cmd = new UpdatePartyCommand(
            Id: partyId,
            Title: "Now a Customer",
            FirstName: null, LastName: null, Email: null, Phone: null, Address: null,
            ContactPerson: null, LoyaltyCode: null, NationalId: null, VatNumber: null,
            TaxId: null, Notes: null,
            PartyType: PartyType.Customer);

        await sut.Handle(cmd, CancellationToken.None);

        var fresh = await Db.Parties.AsNoTracking().FirstAsync(p => p.Id == partyId);
        fresh.PartyType.Should().Be(PartyType.Customer);
    }
}

