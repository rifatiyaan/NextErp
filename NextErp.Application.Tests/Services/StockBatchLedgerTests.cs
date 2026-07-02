using NextErp.Application.Commands;
using NextErp.Application.Commands.SaleReturn;
using NextErp.Application.DTOs.Returns;
using NextErp.Application.Handlers.CommandHandlers.Purchase;
using NextErp.Application.Handlers.CommandHandlers.Sale;
using NextErp.Application.Handlers.CommandHandlers.SaleReturn;
using NextErp.Application.Handlers.CommandHandlers.Stock;
using NextErp.Application.Interfaces;
using NextErp.Application.Services;
using NextErp.Application.Settings;
using NextErp.Application.Tests.Builders;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;
using NextErp.Infrastructure.Services;
using NSubstitute;
using NextErp.Application.DTOs.Purchase;
using SaleDto = NextErp.Application.DTOs.Sale;

namespace NextErp.Application.Tests.Services;

public class StockBatchLedgerTests : HandlerTestBase
{
    private const int VariantA = 51;
    private const int VariantB = 52;
    private const int ProductId = 10;
    private const int CategoryId = 5;

    private SettingsProvider Settings => new(Db);
    private StockService Stocks => new(Db, BranchProvider);

    private CreateSaleHandler BuildSaleHandler() =>
        new(Db, Stocks, BranchProvider, Substitute.For<INotificationService>(), new PricingService(Db), Settings);

    private CreatePurchaseHandler BuildPurchaseHandler() =>
        new(Db, Stocks, BranchProvider, Substitute.For<INotificationService>());

    private CreateStockAdjustmentHandler BuildAdjustmentHandler() =>
        new(Stocks, Db, BranchProvider, Substitute.For<INotificationService>());

    private CreateSaleReturnHandler BuildSaleReturnHandler() =>
        new(Db, Stocks, BranchProvider, Settings);

    private async Task SeedCatalogAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder().WithId(CategoryId).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(ProductId).WithCategory(CategoryId).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(VariantA).WithProduct(ProductId).WithSku("A").WithPrice(100m)
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(VariantB).WithProduct(ProductId).WithSku("B").WithPrice(50m)
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Parties.Add(new PartyBuilder().WithId(Guid.NewGuid()).WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
    }

    private async Task SetConsumptionOrderAsync(InventoryConsumptionOrder order) =>
        await Settings.PatchAsync(
            "Inventory",
            new Dictionary<string, object?> { ["ConsumptionOrder"] = order.ToString() });

    private async Task<Guid> RunPurchaseAsync(int variantId, decimal qty, decimal unitCost)
    {
        var partyId = (await Db.Parties.FirstAsync()).Id;
        var cmd = new CreatePurchaseCommand(
            Title: "Purchase",
            PurchaseNumber: null,
            PartyId: partyId,
            PurchaseDate: DateTime.UtcNow,
            Discount: 0m,
            Metadata: null,
            Items: new List<PurchaseItemRequest>
            {
                new() { ProductVariantId = variantId, Quantity = qty, UnitCost = unitCost },
            });
        return await BuildPurchaseHandler().Handle(cmd, CancellationToken.None);
    }

    private async Task<Guid> RunSaleAsync(int variantId, decimal qty)
    {
        var partyId = (await Db.Parties.FirstAsync()).Id;
        var cmd = new CreateSaleCommand(
            PartyId: partyId,
            Discount: 0m,
            PaymentMethod: null,
            PaidAmount: null,
            Items: new List<SaleDto.SaleItemRequest>
            {
                new() { ProductVariantId = variantId, Quantity = qty },
            });
        return await BuildSaleHandler().Handle(cmd, CancellationToken.None);
    }

    [Fact]
    public async Task Purchase_creates_batch_with_unit_cost_and_link_to_purchase_item()
    {
        await SeedCatalogAsync();

        var purchaseId = await RunPurchaseAsync(VariantA, qty: 10m, unitCost: 25m);

        var batches = await Db.StockBatches.AsNoTracking()
            .Where(b => b.ProductVariantId == VariantA).ToListAsync();
        batches.Should().ContainSingle();
        var batch = batches[0];
        batch.OriginalQuantity.Should().Be(10m);
        batch.RemainingQuantity.Should().Be(10m);
        batch.UnitCost.Should().Be(25m);
        batch.PurchaseItemId.Should().NotBeNull();
    }

    [Fact]
    public async Task Sale_in_Single_mode_does_not_decrement_batches()
    {
        await SeedCatalogAsync();
        await RunPurchaseAsync(VariantA, qty: 10m, unitCost: 25m);
        // Default ConsumptionOrder is Single — no setting change needed.

        await RunSaleAsync(VariantA, qty: 3m);

        var batches = await Db.StockBatches.AsNoTracking()
            .Where(b => b.ProductVariantId == VariantA).ToListAsync();
        batches.Should().ContainSingle();
        batches[0].RemainingQuantity.Should().Be(10m); // batch ledger untouched

        var stock = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantA);
        stock.AvailableQuantity.Should().Be(7m); // Stock still decremented
    }

    [Fact]
    public async Task Sale_in_FIFO_walks_oldest_batch_first()
    {
        await SeedCatalogAsync();
        await SetConsumptionOrderAsync(InventoryConsumptionOrder.Fifo);

        // Two batches arrive in order; the older one (25/unit) should be drained first.
        await RunPurchaseAsync(VariantA, qty: 5m, unitCost: 25m);
        await RunPurchaseAsync(VariantA, qty: 5m, unitCost: 30m);

        await RunSaleAsync(VariantA, qty: 7m); // consumes all 5 of cheap + 2 of pricey

        var byCost = (await Db.StockBatches.AsNoTracking()
            .Where(b => b.ProductVariantId == VariantA)
            .OrderBy(b => b.UnitCost)
            .ToListAsync());
        byCost[0].UnitCost.Should().Be(25m);
        byCost[0].RemainingQuantity.Should().Be(0m);
        byCost[1].UnitCost.Should().Be(30m);
        byCost[1].RemainingQuantity.Should().Be(3m);
    }

    [Fact]
    public async Task Sale_in_LIFO_walks_newest_batch_first()
    {
        await SeedCatalogAsync();
        await SetConsumptionOrderAsync(InventoryConsumptionOrder.Lifo);

        await RunPurchaseAsync(VariantA, qty: 5m, unitCost: 25m);
        await RunPurchaseAsync(VariantA, qty: 5m, unitCost: 30m);

        await RunSaleAsync(VariantA, qty: 7m); // consumes all 5 of newer (30) + 2 of older (25)

        var byCost = (await Db.StockBatches.AsNoTracking()
            .Where(b => b.ProductVariantId == VariantA)
            .OrderBy(b => b.UnitCost)
            .ToListAsync());
        byCost[0].UnitCost.Should().Be(25m);
        byCost[0].RemainingQuantity.Should().Be(3m);
        byCost[1].UnitCost.Should().Be(30m);
        byCost[1].RemainingQuantity.Should().Be(0m);
    }

    [Fact]
    public async Task Sale_invariant_holds_sum_of_batches_equals_available_quantity()
    {
        await SeedCatalogAsync();
        await SetConsumptionOrderAsync(InventoryConsumptionOrder.Fifo);
        await RunPurchaseAsync(VariantA, qty: 4m, unitCost: 25m);
        await RunPurchaseAsync(VariantA, qty: 6m, unitCost: 30m);

        await RunSaleAsync(VariantA, qty: 7m);

        var batchSum = await Db.StockBatches.AsNoTracking()
            .Where(b => b.ProductVariantId == VariantA)
            .SumAsync(b => b.RemainingQuantity);
        var stockQty = (await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantA))
            .AvailableQuantity;
        batchSum.Should().Be(stockQty);
        stockQty.Should().Be(3m);
    }

    [Fact]
    public async Task Adjustment_increase_creates_zero_cost_batch()
    {
        await SeedCatalogAsync();
        // Ensure a Stock row exists first (RecordMovement requires existing or creates one).
        await RunPurchaseAsync(VariantA, qty: 1m, unitCost: 25m);

        await BuildAdjustmentHandler().Handle(new CreateStockAdjustmentCommand(
            ProductVariantId: VariantA,
            Mode: StockAdjustmentMode.Increase,
            Quantity: 4m,
            ReasonCode: "found",
            Notes: null), CancellationToken.None);

        var batches = await Db.StockBatches.AsNoTracking()
            .Where(b => b.ProductVariantId == VariantA)
            .OrderBy(b => b.CreatedAt)
            .ToListAsync();
        batches.Should().HaveCount(2);
        var adjustmentBatch = batches.Single(b => b.PurchaseItemId == null);
        adjustmentBatch.OriginalQuantity.Should().Be(4m);
        adjustmentBatch.UnitCost.Should().Be(0m);
    }

    private async Task<Guid> RunSaleReturnAsync(Guid saleId, decimal qty)
    {
        var saleItem = await Db.SaleItems.AsNoTracking().FirstAsync(i => i.SaleId == saleId);
        var dto = new CreateSaleReturnRequest
        {
            SaleId = saleId,
            ReturnDate = DateTime.UtcNow,
            Reason = "test",
            Items = new List<SaleReturnLineRequest>
            {
                new()
                {
                    SaleItemId = saleItem.Id,
                    ProductVariantId = saleItem.ProductVariantId,
                    Quantity = qty,
                },
            },
        };
        return await BuildSaleReturnHandler().Handle(new CreateSaleReturnCommand(dto), CancellationToken.None);
    }

    [Fact]
    public async Task FIFO_sale_stamps_UnitCostAtSale_from_consumed_batches()
    {
        await SeedCatalogAsync();
        await SetConsumptionOrderAsync(InventoryConsumptionOrder.Fifo);
        await RunPurchaseAsync(VariantA, qty: 4m, unitCost: 25m);
        await RunPurchaseAsync(VariantA, qty: 4m, unitCost: 30m);

        var saleId = await RunSaleAsync(VariantA, qty: 6m); // 4×25 + 2×30 = 160, /6 = 26.6667

        var saleItem = await Db.SaleItems.AsNoTracking().FirstAsync(i => i.SaleId == saleId);
        saleItem.UnitCostAtSale.Should().NotBeNull();
        saleItem.UnitCostAtSale!.Value.Should().BeApproximately(26.6667m, 0.0001m);
    }

    [Fact]
    public async Task LIFO_sale_stamps_different_UnitCostAtSale_than_FIFO()
    {
        await SeedCatalogAsync();
        await SetConsumptionOrderAsync(InventoryConsumptionOrder.Lifo);
        await RunPurchaseAsync(VariantA, qty: 4m, unitCost: 25m);
        await RunPurchaseAsync(VariantA, qty: 4m, unitCost: 30m);

        var saleId = await RunSaleAsync(VariantA, qty: 6m); // 4×30 + 2×25 = 170, /6 = 28.3333

        var saleItem = await Db.SaleItems.AsNoTracking().FirstAsync(i => i.SaleId == saleId);
        saleItem.UnitCostAtSale!.Value.Should().BeApproximately(28.3333m, 0.0001m);
    }

    [Fact]
    public async Task Single_mode_sale_leaves_UnitCostAtSale_null()
    {
        await SeedCatalogAsync();
        // Default ConsumptionOrder is Single — no setting change needed.
        await RunPurchaseAsync(VariantA, qty: 4m, unitCost: 25m);

        var saleId = await RunSaleAsync(VariantA, qty: 2m);

        var saleItem = await Db.SaleItems.AsNoTracking().FirstAsync(i => i.SaleId == saleId);
        saleItem.UnitCostAtSale.Should().BeNull();
    }

    [Fact]
    public async Task Sale_return_under_FIFO_creates_return_batch_at_original_cost()
    {
        await SeedCatalogAsync();
        await SetConsumptionOrderAsync(InventoryConsumptionOrder.Fifo);
        await RunPurchaseAsync(VariantA, qty: 5m, unitCost: 25m);

        var saleId = await RunSaleAsync(VariantA, qty: 3m); // consumes 3 × $25 from the batch

        await RunSaleReturnAsync(saleId, qty: 2m);

        // After the sale, the original batch had 2 units left at $25. The
        // return should produce a NEW batch with 2 units at $25 (cost-preserving),
        // not just bump the existing batch back up.
        var batches = await Db.StockBatches.AsNoTracking()
            .Where(b => b.ProductVariantId == VariantA)
            .OrderBy(b => b.ReceivedAt)
            .ToListAsync();
        batches.Should().HaveCount(2);
        batches[0].UnitCost.Should().Be(25m);   // original purchase batch
        batches[0].RemainingQuantity.Should().Be(2m); // 5 − 3 consumed
        batches[1].UnitCost.Should().Be(25m);   // return batch at preserved cost
        batches[1].RemainingQuantity.Should().Be(2m);
        batches[1].PurchaseItemId.Should().BeNull(); // not from a purchase
    }

    [Fact]
    public async Task Sale_return_under_Single_does_not_create_return_batch()
    {
        await SeedCatalogAsync();
        // ConsumptionOrder remains Single.
        await RunPurchaseAsync(VariantA, qty: 5m, unitCost: 25m);
        var saleId = await RunSaleAsync(VariantA, qty: 3m);

        await RunSaleReturnAsync(saleId, qty: 2m);

        // In Single mode the return just bumps Stock.AvailableQuantity. The
        // original purchase batch is still untouched (Single never consumed).
        var batches = await Db.StockBatches.AsNoTracking()
            .Where(b => b.ProductVariantId == VariantA)
            .ToListAsync();
        batches.Should().ContainSingle();
        var stock = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantA);
        stock.AvailableQuantity.Should().Be(4m); // 5 − 3 + 2
    }

    [Fact]
    public async Task Sale_return_invariant_holds_after_FIFO_sale_and_return()
    {
        await SeedCatalogAsync();
        await SetConsumptionOrderAsync(InventoryConsumptionOrder.Fifo);
        await RunPurchaseAsync(VariantA, qty: 4m, unitCost: 25m);
        await RunPurchaseAsync(VariantA, qty: 4m, unitCost: 30m);

        var saleId = await RunSaleAsync(VariantA, qty: 6m);
        await RunSaleReturnAsync(saleId, qty: 2m);

        var stock = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantA);
        var batchSum = await Db.StockBatches.AsNoTracking()
            .Where(b => b.ProductVariantId == VariantA)
            .SumAsync(b => b.RemainingQuantity);
        stock.AvailableQuantity.Should().Be(4m); // 8 − 6 + 2
        batchSum.Should().Be(stock.AvailableQuantity);
    }

    [Fact]
    public async Task Adjustment_decrease_walks_FIFO_regardless_of_consumption_setting()
    {
        await SeedCatalogAsync();
        await SetConsumptionOrderAsync(InventoryConsumptionOrder.Lifo);
        await RunPurchaseAsync(VariantA, qty: 4m, unitCost: 25m);
        await RunPurchaseAsync(VariantA, qty: 4m, unitCost: 30m);

        await BuildAdjustmentHandler().Handle(new CreateStockAdjustmentCommand(
            ProductVariantId: VariantA,
            Mode: StockAdjustmentMode.Decrease,
            Quantity: 5m,
            ReasonCode: "shrinkage",
            Notes: null), CancellationToken.None);

        // FIFO: older 25-cost batch drained first regardless of Lifo setting.
        var byCost = await Db.StockBatches.AsNoTracking()
            .Where(b => b.ProductVariantId == VariantA)
            .OrderBy(b => b.UnitCost)
            .ToListAsync();
        byCost[0].UnitCost.Should().Be(25m);
        byCost[0].RemainingQuantity.Should().Be(0m);
        byCost[1].UnitCost.Should().Be(30m);
        byCost[1].RemainingQuantity.Should().Be(3m);
    }
}
