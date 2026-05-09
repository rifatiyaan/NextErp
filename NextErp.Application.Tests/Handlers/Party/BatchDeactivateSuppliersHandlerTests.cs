using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Party;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Party;

public class BatchDeactivateSuppliersHandlerTests : HandlerTestBase
{
    private BatchDeactivateSuppliersHandler BuildHandler() => new(Db);

    private async Task SeedBranchAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        await Db.SaveChangesAsync();
    }

    private async Task<Guid> SeedSupplierAsync(string title = "S", bool active = true)
    {
        var id = Guid.NewGuid();
        var party = new PartyBuilder()
            .WithId(id).WithTitle(title).AsSupplier()
            .WithTenant(TenantId).WithBranch(BranchId).Build();
        if (!active) party.IsActive = false;
        Db.Parties.Add(party);
        await Db.SaveChangesAsync();
        return id;
    }

    private async Task<Guid> SeedCustomerAsync(string title = "C")
    {
        var id = Guid.NewGuid();
        Db.Parties.Add(new PartyBuilder()
            .WithId(id).WithTitle(title)
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
        return id;
    }

    [Fact]
    public async Task Happy_path_deactivates_active_supplier_rows()
    {
        await SeedBranchAsync();
        var a = await SeedSupplierAsync("Alpha");
        var b = await SeedSupplierAsync("Beta");
        var c = await SeedSupplierAsync("Gamma");
        var before = DateTime.UtcNow;
        var sut = BuildHandler();

        var count = await sut.Handle(
            new BatchDeactivateSuppliersCommand(new[] { a, b, c }),
            CancellationToken.None);

        count.Should().Be(3);
        // Party is [BranchScoped]; bypass the global filter which AND-s IsActive.
        var rows = await Db.Parties.AsNoTracking()
            .IgnoreQueryFilters()
            .Where(p => new[] { a, b, c }.Contains(p.Id)).ToListAsync();
        rows.Should().HaveCount(3);
        rows.Should().OnlyContain(p => p.IsActive == false);
        rows.Should().OnlyContain(p =>
            p.UpdatedAt != null && p.UpdatedAt!.Value >= before);
    }

    [Fact]
    public async Task Customer_id_is_ignored_when_running_suppliers_batch()
    {
        // Mirror of the customers test — operator runs "deactivate suppliers" against a
        // list that accidentally contains a Customer id. PartyType filter protects us.
        await SeedBranchAsync();
        var customerId = await SeedCustomerAsync();
        var sut = BuildHandler();

        var count = await sut.Handle(
            new BatchDeactivateSuppliersCommand(new[] { customerId }),
            CancellationToken.None);

        count.Should().Be(0);
        var fresh = await Db.Parties.AsNoTracking()
            .IgnoreQueryFilters()
            .FirstAsync(p => p.Id == customerId);
        fresh.IsActive.Should().BeTrue();
        fresh.PartyType.Should().Be(PartyType.Customer);
    }

    [Fact]
    public async Task Pre_inactive_supplier_skipped_in_count()
    {
        await SeedBranchAsync();
        var active = await SeedSupplierAsync("Active");
        var inactive = await SeedSupplierAsync("Inactive", active: false);
        var sut = BuildHandler();

        var count = await sut.Handle(
            new BatchDeactivateSuppliersCommand(new[] { active, inactive }),
            CancellationToken.None);

        count.Should().Be(1);
    }
}
