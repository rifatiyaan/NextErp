using NextErp.Application.Handlers.QueryHandlers.Stock;
using NextErp.Application.Queries;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Stock;

public class GetStockAdjustmentHistoryHandlerTests : HandlerTestBase
{
    private GetStockAdjustmentHistoryHandler BuildHandler() => new(Db);

    private const int VariantA = 700;
    private const int VariantB = 701;

    private async Task SeedAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder()
            .WithId(700).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(700).WithCategory(700).WithTitle("Stock Product")
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(VariantA).WithProduct(700).WithSku("ADJ-A")
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(VariantB).WithProduct(700).WithSku("ADJ-B")
            .WithTenant(TenantId).WithBranch(BranchId).Build());

        var stockA = new StockBuilder()
            .WithVariant(VariantA).WithAvailable(10m).WithTenant(TenantId).WithBranch(BranchId).Build();
        var stockB = new StockBuilder()
            .WithVariant(VariantB).WithAvailable(20m).WithTenant(TenantId).WithBranch(BranchId).Build();
        Db.Stocks.Add(stockA);
        Db.Stocks.Add(stockB);
        await Db.SaveChangesAsync();

        // Mix of movement types for VariantA and VariantB.
        AddMovement(stockA.Id, VariantA, type: StockMovementType.ManualAdjustment, qty: 2m, reason: "FOUND");
        AddMovement(stockA.Id, VariantA, type: StockMovementType.Sale, qty: -1m);
        AddMovement(stockB.Id, VariantB, type: StockMovementType.ManualAdjustment, qty: -3m, reason: "DAMAGED");
        AddMovement(stockB.Id, VariantB, type: StockMovementType.Purchase, qty: 5m);
        await Db.SaveChangesAsync();
    }

    private void AddMovement(Guid stockId, int variantId, StockMovementType type, decimal qty, string? reason = null)
    {
        Db.StockMovements.Add(new StockMovement
        {
            Id = Guid.NewGuid(),
            StockId = stockId,
            ProductVariantId = variantId,
            BranchId = BranchId,
            QuantityChanged = qty,
            PreviousQuantity = 0m,
            NewQuantity = qty,
            MovementType = type,
            ReferenceId = Guid.NewGuid(),
            Reason = reason,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        });
    }

    [Fact]
    public async Task Only_ManualAdjustment_movements_returned()
    {
        await SeedAsync();
        var sut = BuildHandler();

        var page = await sut.Handle(
            new GetStockAdjustmentHistoryQuery(ProductVariantId: null),
            CancellationToken.None);

        page.Total.Should().Be(2);
        page.Items.Should().HaveCount(2);
        page.Items.Select(i => i.ReasonCode).Should().BeEquivalentTo(new[] { "FOUND", "DAMAGED" });
    }

    [Fact]
    public async Task ProductVariantId_filter_returns_only_matching_variant()
    {
        await SeedAsync();
        var sut = BuildHandler();

        var page = await sut.Handle(
            new GetStockAdjustmentHistoryQuery(ProductVariantId: VariantB),
            CancellationToken.None);

        page.Total.Should().Be(1);
        page.Items.Should().ContainSingle()
            .Which.ReasonCode.Should().Be("DAMAGED");
    }
}

