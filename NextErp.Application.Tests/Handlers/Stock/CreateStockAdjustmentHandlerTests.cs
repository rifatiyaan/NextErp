using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Stock;
using NextErp.Application.Services;
using NextErp.Domain.Entities;
using NSubstitute;

namespace NextErp.Application.Tests.Handlers.Stock;

/// <summary>
/// Verifies CreateStockAdjustmentHandler against a real SQLite-backed StockService.
/// We deliberately exercise the *real* service so the StockMovement insert + Stock update flow
/// is covered end-to-end (the regression surface after the UoW removal).
/// </summary>
public class CreateStockAdjustmentHandlerTests : HandlerTestBase
{
    private const int VariantId = 1;

    private CreateStockAdjustmentHandler BuildHandler()
    {
        var service = new StockService(Db, BranchProvider);
        return new CreateStockAdjustmentHandler(service, Db, BranchProvider);
    }

    private async Task SeedVariantWithStockAsync(decimal initialAvailable = 10m)
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder().WithId(1).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(1).WithCategory(1).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(VariantId).WithProduct(1).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Stocks.Add(new StockBuilder()
            .WithVariant(VariantId).WithAvailable(initialAvailable).WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Increase_mode_adds_quantity_and_writes_movement()
    {
        await SeedVariantWithStockAsync(10m);
        var sut = BuildHandler();

        var cmd = new CreateStockAdjustmentCommand(
            VariantId,
            StockAdjustmentMode.Increase,
            5m,
            StockAdjustmentReason.Damaged,
            "test");

        var movementId = await sut.Handle(cmd, CancellationToken.None);

        movementId.Should().NotBe(Guid.Empty);

        var stock = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantId);
        stock.AvailableQuantity.Should().Be(15m);

        var movements = await Db.StockMovements.AsNoTracking().ToListAsync();
        movements.Should().ContainSingle();
        movements[0].MovementType.Should().Be(StockMovementType.ManualAdjustment);
        movements[0].QuantityChanged.Should().Be(5m);
        movements[0].PreviousQuantity.Should().Be(10m);
        movements[0].NewQuantity.Should().Be(15m);
        movements[0].Reason.Should().Be(StockAdjustmentReason.Damaged);
        movements[0].Notes.Should().Be("test");
    }

    [Fact]
    public async Task Decrease_mode_subtracts_quantity_and_writes_negative_movement()
    {
        await SeedVariantWithStockAsync(10m);
        var sut = BuildHandler();

        var cmd = new CreateStockAdjustmentCommand(
            VariantId,
            StockAdjustmentMode.Decrease,
            3m,
            StockAdjustmentReason.Damaged,
            null);

        await sut.Handle(cmd, CancellationToken.None);

        var stock = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantId);
        stock.AvailableQuantity.Should().Be(7m);

        var movement = await Db.StockMovements.AsNoTracking().SingleAsync();
        movement.QuantityChanged.Should().Be(-3m);
        movement.MovementType.Should().Be(StockMovementType.ManualAdjustment);
    }

    [Fact]
    public async Task SetAbsolute_mode_writes_delta_to_target()
    {
        await SeedVariantWithStockAsync(10m);
        var sut = BuildHandler();

        var cmd = new CreateStockAdjustmentCommand(
            VariantId,
            StockAdjustmentMode.SetAbsolute,
            25m,
            StockAdjustmentReason.PhysicalCountCorrection,
            null);

        await sut.Handle(cmd, CancellationToken.None);

        var stock = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantId);
        stock.AvailableQuantity.Should().Be(25m);

        var movement = await Db.StockMovements.AsNoTracking().SingleAsync();
        movement.QuantityChanged.Should().Be(15m);
    }

    [Fact]
    public async Task Decrease_beyond_available_throws_negative_stock()
    {
        await SeedVariantWithStockAsync(5m);
        var sut = BuildHandler();

        var cmd = new CreateStockAdjustmentCommand(
            VariantId,
            StockAdjustmentMode.Decrease,
            10m,
            StockAdjustmentReason.Damaged,
            null);

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*negative stock*");
    }

    [Fact]
    public async Task SetAbsolute_to_current_value_throws_no_change()
    {
        await SeedVariantWithStockAsync(10m);
        var sut = BuildHandler();

        var cmd = new CreateStockAdjustmentCommand(
            VariantId,
            StockAdjustmentMode.SetAbsolute,
            10m,
            StockAdjustmentReason.PhysicalCountCorrection,
            null);

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*No change*");
    }

    [Fact]
    public async Task Variant_not_found_throws()
    {
        await SeedVariantWithStockAsync(10m);
        var sut = BuildHandler();

        var cmd = new CreateStockAdjustmentCommand(
            ProductVariantId: 999,
            StockAdjustmentMode.Increase,
            1m,
            StockAdjustmentReason.Damaged,
            null);

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Branch_context_required()
    {
        await SeedVariantWithStockAsync(10m);
        // Override default: simulate missing branch claim.
        BranchProvider.GetBranchId().Returns((Guid?)null);

        var sut = BuildHandler();

        var cmd = new CreateStockAdjustmentCommand(
            VariantId,
            StockAdjustmentMode.Increase,
            1m,
            StockAdjustmentReason.Damaged,
            null);

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*Branch context*");
    }

    [Fact]
    public async Task Reason_and_notes_are_persisted_to_movement()
    {
        await SeedVariantWithStockAsync(10m);
        var sut = BuildHandler();

        var cmd = new CreateStockAdjustmentCommand(
            VariantId,
            StockAdjustmentMode.Increase,
            2m,
            StockAdjustmentReason.OpeningBalance,
            "imported from legacy system");

        await sut.Handle(cmd, CancellationToken.None);

        var movement = await Db.StockMovements.AsNoTracking().SingleAsync();
        movement.Reason.Should().Be(StockAdjustmentReason.OpeningBalance);
        movement.Notes.Should().Be("imported from legacy system");
    }
}
