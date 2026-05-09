using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Party;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Party;

public class BatchDeactivateCustomersHandlerTests : HandlerTestBase
{
    private BatchDeactivateCustomersHandler BuildHandler() => new(Db);

    private async Task SeedBranchAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        await Db.SaveChangesAsync();
    }

    private async Task<Guid> SeedCustomerAsync(string title = "Cust", bool active = true)
    {
        var id = Guid.NewGuid();
        var party = new PartyBuilder()
            .WithId(id).WithTitle(title).WithTenant(TenantId).WithBranch(BranchId).Build();
        if (!active) party.IsActive = false;
        Db.Parties.Add(party);
        await Db.SaveChangesAsync();
        return id;
    }

    private async Task<Guid> SeedSupplierAsync(string title = "Supp")
    {
        var id = Guid.NewGuid();
        Db.Parties.Add(new PartyBuilder()
            .WithId(id).WithTitle(title).AsSupplier()
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
        return id;
    }

    [Fact]
    public async Task Happy_path_deactivates_active_customer_rows()
    {
        await SeedBranchAsync();
        var a = await SeedCustomerAsync("A");
        var b = await SeedCustomerAsync("B");
        var c = await SeedCustomerAsync("C");
        var before = DateTime.UtcNow;
        var sut = BuildHandler();

        var count = await sut.Handle(
            new BatchDeactivateCustomersCommand(new[] { a, b, c }),
            CancellationToken.None);

        count.Should().Be(3);
        // Party is [BranchScoped] — verification must IgnoreQueryFilters() since the
        // global filter ANDs IsActive and would otherwise hide the just-deactivated rows.
        var rows = await Db.Parties.AsNoTracking()
            .IgnoreQueryFilters()
            .Where(p => new[] { a, b, c }.Contains(p.Id)).ToListAsync();
        rows.Should().HaveCount(3);
        rows.Should().OnlyContain(p => p.IsActive == false);
        rows.Should().OnlyContain(p =>
            p.UpdatedAt != null && p.UpdatedAt!.Value >= before);
    }

    [Fact]
    public async Task Pre_inactive_customer_is_skipped_in_count()
    {
        await SeedBranchAsync();
        var active = await SeedCustomerAsync("Active");
        var inactive = await SeedCustomerAsync("Inactive", active: false);
        var sut = BuildHandler();

        var count = await sut.Handle(
            new BatchDeactivateCustomersCommand(new[] { active, inactive }),
            CancellationToken.None);

        count.Should().Be(1);
    }

    [Fact]
    public async Task Supplier_id_is_ignored_when_running_customers_batch()
    {
        // Id-collision safety: the operator runs "deactivate customers" but accidentally
        // includes a Supplier id. The PartyType filter must keep the supplier untouched.
        await SeedBranchAsync();
        var supplierId = await SeedSupplierAsync();
        var sut = BuildHandler();

        var count = await sut.Handle(
            new BatchDeactivateCustomersCommand(new[] { supplierId }),
            CancellationToken.None);

        count.Should().Be(0);
        var fresh = await Db.Parties.AsNoTracking()
            .IgnoreQueryFilters()
            .FirstAsync(p => p.Id == supplierId);
        fresh.IsActive.Should().BeTrue();
        fresh.PartyType.Should().Be(PartyType.Supplier);
    }
}
