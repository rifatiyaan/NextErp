using NextErp.Application.Commands;
using NextErp.Application.DTOs;
using NextErp.Application.Handlers.CommandHandlers.Sale;
using NextErp.Application.Services;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Sale;

/// <summary>
/// Verifies CreateSaleHandler against a real SQLite-backed StockService.
/// Critical regression target after the UoW removal: ensures atomic Sale + SaleItems +
/// StockMovements + Stock decrement, and that no rows persist on insufficient-stock.
/// </summary>
public class CreateSaleHandlerTests : HandlerTestBase
{
    private CreateSaleHandler BuildHandler()
    {
        var service = new StockService(Db, BranchProvider);
        return new CreateSaleHandler(Db, service, BranchProvider);
    }

    private const int VariantA = 1;
    private const int VariantB = 2;

    private async Task<Guid> SeedAsync(decimal stockA = 10m, decimal stockB = 10m, decimal priceA = 100m, decimal priceB = 50m)
    {
        var partyId = Guid.NewGuid();
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder().WithId(1).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(1).WithCategory(1).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(VariantA).WithProduct(1).WithSku("A").WithPrice(priceA)
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(VariantB).WithProduct(1).WithSku("B").WithPrice(priceB)
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Stocks.Add(new StockBuilder()
            .WithVariant(VariantA).WithAvailable(stockA).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Stocks.Add(new StockBuilder()
            .WithVariant(VariantB).WithAvailable(stockB).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Parties.Add(new PartyBuilder().WithId(partyId).WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
        return partyId;
    }

    [Fact]
    public async Task Happy_path_creates_sale_items_movements_and_decrements_stock()
    {
        var partyId = await SeedAsync(stockA: 10m, stockB: 10m, priceA: 100m, priceB: 50m);
        var sut = BuildHandler();

        var cmd = new CreateSaleCommand(
            PartyId: partyId,
            Discount: 0m,
            PaymentMethod: null,
            PaidAmount: null,
            Items: new List<DTOs.Sale.Request.Create.SaleItemRequest>
            {
                new() { ProductVariantId = VariantA, Quantity = 2m },
                new() { ProductVariantId = VariantB, Quantity = 3m },
            });

        var saleId = await sut.Handle(cmd, CancellationToken.None);

        saleId.Should().NotBe(Guid.Empty);

        var sale = await Db.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);
        sale.PartyId.Should().Be(partyId);

        var items = await Db.SaleItems.AsNoTracking().Where(i => i.SaleId == saleId).ToListAsync();
        items.Should().HaveCount(2);
        items.Should().Contain(i => i.ProductVariantId == VariantA && i.Quantity == 2m && i.Price == 100m);
        items.Should().Contain(i => i.ProductVariantId == VariantB && i.Quantity == 3m && i.Price == 50m);

        var movements = await Db.StockMovements.AsNoTracking().Where(m => m.ReferenceId == saleId).ToListAsync();
        movements.Should().HaveCount(2);
        movements.Should().AllSatisfy(m => m.MovementType.Should().Be(StockMovementType.Sale));
        movements.Should().Contain(m => m.ProductVariantId == VariantA && m.QuantityChanged == -2m);
        movements.Should().Contain(m => m.ProductVariantId == VariantB && m.QuantityChanged == -3m);

        var stockA = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantA);
        var stockB = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantB);
        stockA.AvailableQuantity.Should().Be(8m);
        stockB.AvailableQuantity.Should().Be(7m);
    }

    [Fact]
    public async Task Insufficient_stock_fails_atomically_no_rows_persisted()
    {
        // Single-line sale where requested qty exceeds available — verify atomicity.
        var partyId = await SeedAsync(stockA: 5m, stockB: 5m);
        var sut = BuildHandler();

        var cmd = new CreateSaleCommand(
            PartyId: partyId,
            Discount: 0m,
            PaymentMethod: null,
            PaidAmount: null,
            Items: new List<DTOs.Sale.Request.Create.SaleItemRequest>
            {
                new() { ProductVariantId = VariantA, Quantity = 10m },
            });

        var act = async () => await sut.Handle(cmd, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        (await Db.Sales.AsNoTracking().AnyAsync()).Should().BeFalse();
        (await Db.SaleItems.AsNoTracking().AnyAsync()).Should().BeFalse();
        (await Db.StockMovements.AsNoTracking().AnyAsync()).Should().BeFalse();

        var stockA = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantA);
        stockA.AvailableQuantity.Should().Be(5m);
    }

    [Fact]
    public async Task Multiple_items_one_fails_no_rows_persisted_and_no_stock_change()
    {
        // First line is fine; second line over-draws. The whole save should fail.
        var partyId = await SeedAsync(stockA: 10m, stockB: 1m);
        var sut = BuildHandler();

        var cmd = new CreateSaleCommand(
            PartyId: partyId,
            Discount: 0m,
            PaymentMethod: null,
            PaidAmount: null,
            Items: new List<DTOs.Sale.Request.Create.SaleItemRequest>
            {
                new() { ProductVariantId = VariantA, Quantity = 2m },
                new() { ProductVariantId = VariantB, Quantity = 5m },
            });

        var act = async () => await sut.Handle(cmd, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        (await Db.Sales.AsNoTracking().AnyAsync()).Should().BeFalse();
        (await Db.SaleItems.AsNoTracking().AnyAsync()).Should().BeFalse();
        (await Db.StockMovements.AsNoTracking().AnyAsync()).Should().BeFalse();

        var stockA = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantA);
        var stockB = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantB);
        stockA.AvailableQuantity.Should().Be(10m);
        stockB.AvailableQuantity.Should().Be(1m);
    }
}
