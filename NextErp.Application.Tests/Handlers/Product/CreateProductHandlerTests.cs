using AutoMapper;
using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Product;
using NextErp.Application.Services;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Product;

public class CreateProductHandlerTests : HandlerTestBase
{
    private static readonly IMapper Mapper = BuildMapper();

    private static IMapper BuildMapper()
    {
        var cfg = new MapperConfiguration(c =>
            c.AddMaps(typeof(NextErp.Application.ApplicationAssemblyMarker).Assembly));
        return cfg.CreateMapper();
    }

    private const int CategoryIdA = 200;

    private CreateProductHandler BuildHandler()
    {
        var stockService = new StockService(Db, BranchProvider);
        return new CreateProductHandler(Db, stockService, BranchProvider, Mapper);
    }

    private async Task SeedAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder()
            .WithId(CategoryIdA).WithTitle("Cat").WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
    }

    private static CreateProductCommand BuildCommand(
        decimal initialStock = 0m,
        decimal price = 100m,
        string code = "PROD-001",
        string title = "New Product")
        => new(
            Title: title,
            Code: code,
            ParentId: null,
            CategoryId: CategoryIdA,
            Price: price,
            InitialStock: initialStock);

    [Fact]
    public async Task Happy_path_creates_product_and_returns_id()
    {
        await SeedAsync();
        var sut = BuildHandler();

        var id = await sut.Handle(BuildCommand(), CancellationToken.None);

        id.Should().BeGreaterThan(0);

        var fresh = await Db.Products.AsNoTracking().FirstAsync(p => p.Id == id);
        fresh.Title.Should().Be("New Product");
        fresh.Code.Should().Be("PROD-001");
        fresh.IsActive.Should().BeTrue();
        fresh.HasVariations.Should().BeFalse();

        // Default ProductVariant created from SimpleProductVariantFactory.
        var variants = await Db.ProductVariants.AsNoTracking()
            .Where(v => v.ProductId == id).ToListAsync();
        variants.Should().ContainSingle();
        variants[0].Sku.Should().Be("PROD-001-DEFAULT");
    }

    // Regression test for the duplicate-Stock-row bug: CreateProductHandler stages a Stock
    // row via EnsureStockRecordExistsAsync, then calls SetAvailableQuantityAsync before
    // SaveChanges. Earlier the second call missed the staged Added entity and inserted a
    // duplicate, tripping UNIQUE(ProductVariantId, BranchId). Fixed in StockService by
    // checking DbSet.Local first in GetTrackedStockByVariantAndBranchAsync.
    [Fact]
    public async Task With_initial_stock_seeds_stock_row_and_movement()
    {
        await SeedAsync();
        var sut = BuildHandler();

        var id = await sut.Handle(
            BuildCommand(initialStock: 25m, code: "PROD-002"),
            CancellationToken.None);

        var variant = await Db.ProductVariants.AsNoTracking().FirstAsync(v => v.ProductId == id);

        var stock = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == variant.Id);
        stock.AvailableQuantity.Should().Be(25m);
        stock.BranchId.Should().Be(BranchId);

        // SetAvailableQuantityAsync routes through IncreaseStockAsync, which writes a
        // ManualAdjustment movement (not Purchase). FLAG above.
        var movements = await Db.StockMovements.AsNoTracking()
            .Where(m => m.ProductVariantId == variant.Id).ToListAsync();
        movements.Should().ContainSingle();
        movements[0].MovementType.Should().Be(StockMovementType.ManualAdjustment);
        movements[0].QuantityChanged.Should().Be(25m);
        movements[0].PreviousQuantity.Should().Be(0m);
        movements[0].NewQuantity.Should().Be(25m);
    }

    [Fact]
    public async Task Branch_and_tenant_are_set_on_persisted_product()
    {
        await SeedAsync();
        var sut = BuildHandler();

        var id = await sut.Handle(BuildCommand(code: "PROD-003"), CancellationToken.None);

        var fresh = await Db.Products.AsNoTracking().FirstAsync(p => p.Id == id);
        fresh.BranchId.Should().Be(BranchId);
        fresh.TenantId.Should().Be(TenantId);
    }
}

