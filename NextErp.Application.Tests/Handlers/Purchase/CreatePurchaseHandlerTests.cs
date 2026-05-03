using NextErp.Application.Commands;
using NextErp.Application.DTOs;
using NextErp.Application.Handlers.CommandHandlers.Purchase;
using NextErp.Application.Services;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Purchase;

public class CreatePurchaseHandlerTests : HandlerTestBase
{
    private CreatePurchaseHandler BuildHandler()
    {
        var service = new StockService(Db, BranchProvider);
        return new CreatePurchaseHandler(Db, service, BranchProvider);
    }

    private const int VariantA = 100;
    private const int VariantB = 101;

    private async Task<Guid> SeedAsync(decimal stockA = 10m, decimal stockB = 10m, decimal priceA = 50m, decimal priceB = 25m)
    {
        var partyId = Guid.NewGuid();
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder().WithId(100).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(100).WithCategory(100).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(VariantA).WithProduct(100).WithSku("PA").WithPrice(priceA)
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(VariantB).WithProduct(100).WithSku("PB").WithPrice(priceB)
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Stocks.Add(new StockBuilder()
            .WithVariant(VariantA).WithAvailable(stockA).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Stocks.Add(new StockBuilder()
            .WithVariant(VariantB).WithAvailable(stockB).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Parties.Add(new PartyBuilder()
            .WithId(partyId).AsSupplier().WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
        return partyId;
    }

    [Fact]
    public async Task Happy_path_creates_purchase_items_movements_and_increments_stock()
    {
        var partyId = await SeedAsync(stockA: 10m, stockB: 10m);
        var sut = BuildHandler();

        var cmd = new CreatePurchaseCommand(
            Title: "Purchase from supplier",
            PurchaseNumber: "PUR-001",
            PartyId: partyId,
            PurchaseDate: DateTime.UtcNow,
            Discount: 0m,
            Items: new List<DTOs.Purchase.Request.Create.PurchaseItemRequest>
            {
                new() { Title = "Item A", ProductVariantId = VariantA, Quantity = 5m, UnitCost = 40m },
                new() { Title = "Item B", ProductVariantId = VariantB, Quantity = 7m, UnitCost = 20m },
            },
            Metadata: null);

        var purchaseId = await sut.Handle(cmd, CancellationToken.None);

        purchaseId.Should().NotBe(Guid.Empty);
        var purchase = await Db.Purchases.AsNoTracking().FirstAsync(p => p.Id == purchaseId);
        purchase.PartyId.Should().Be(partyId);

        var items = await Db.PurchaseItems.AsNoTracking().Where(i => i.PurchaseId == purchaseId).ToListAsync();
        items.Should().HaveCount(2);
        items.Should().Contain(i => i.ProductVariantId == VariantA && i.Quantity == 5m && i.UnitCost == 40m);

        var movements = await Db.StockMovements.AsNoTracking().Where(m => m.ReferenceId == purchaseId).ToListAsync();
        movements.Should().HaveCount(2);
        movements.Should().AllSatisfy(m => m.MovementType.Should().Be(StockMovementType.Purchase));
        movements.Should().Contain(m => m.ProductVariantId == VariantA && m.QuantityChanged == 5m);
        movements.Should().Contain(m => m.ProductVariantId == VariantB && m.QuantityChanged == 7m);

        var stockA = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantA);
        var stockB = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantB);
        stockA.AvailableQuantity.Should().Be(15m);
        stockB.AvailableQuantity.Should().Be(17m);
    }

    [Fact]
    public async Task Empty_items_throws_invalid_operation()
    {
        await SeedAsync();
        var sut = BuildHandler();

        var cmd = new CreatePurchaseCommand(
            Title: "x",
            PurchaseNumber: "PUR-002",
            PartyId: null,
            PurchaseDate: DateTime.UtcNow,
            Discount: 0m,
            Items: new List<DTOs.Purchase.Request.Create.PurchaseItemRequest>(),
            Metadata: null);

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*at least one*");
    }

    [Fact]
    public async Task Variant_not_found_throws()
    {
        await SeedAsync();
        var sut = BuildHandler();

        var cmd = new CreatePurchaseCommand(
            Title: "x",
            PurchaseNumber: "PUR-003",
            PartyId: null,
            PurchaseDate: DateTime.UtcNow,
            Discount: 0m,
            Items: new List<DTOs.Purchase.Request.Create.PurchaseItemRequest>
            {
                new() { Title = "Missing variant", ProductVariantId = 9999, Quantity = 1m, UnitCost = 1m },
            },
            Metadata: null);

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*9999*");
    }

    [Fact]
    public async Task Multiple_lines_for_same_variant_increment_stock_by_sum()
    {
        await SeedAsync(stockA: 10m);
        var sut = BuildHandler();

        var cmd = new CreatePurchaseCommand(
            Title: "Stack purchase",
            PurchaseNumber: "PUR-004",
            PartyId: null,
            PurchaseDate: DateTime.UtcNow,
            Discount: 0m,
            Items: new List<DTOs.Purchase.Request.Create.PurchaseItemRequest>
            {
                new() { Title = "Line 1", ProductVariantId = VariantA, Quantity = 3m, UnitCost = 10m },
                new() { Title = "Line 2", ProductVariantId = VariantA, Quantity = 4m, UnitCost = 10m },
            },
            Metadata: null);

        await sut.Handle(cmd, CancellationToken.None);

        var stockA = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantA);
        stockA.AvailableQuantity.Should().Be(17m); // 10 + 3 + 4 (sum, not last)
    }
}

