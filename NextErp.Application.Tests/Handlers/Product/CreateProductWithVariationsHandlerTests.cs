using AutoMapper;
using AppDtos = NextErp.Application.DTOs;
using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Product;
using NextErp.Application.Services;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Product;

public class CreateProductWithVariationsHandlerTests : HandlerTestBase
{
    private static readonly IMapper Mapper = BuildMapper();

    private static IMapper BuildMapper()
    {
        var cfg = new MapperConfiguration(c =>
            c.AddMaps(typeof(NextErp.Application.ApplicationAssemblyMarker).Assembly));
        return cfg.CreateMapper();
    }

    private const int CategoryId = 300;
    private const int OptSizeId = 300;
    private const int OptColorId = 301;

    private CreateProductWithVariationsHandler BuildHandler()
    {
        var stockService = new StockService(Db, BranchProvider);
        return new CreateProductWithVariationsHandler(Db, stockService, BranchProvider, Mapper);
    }

    private async Task SeedSchemaAsync(bool seedColorOption = true)
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder()
            .WithId(CategoryId).WithTitle("Cat").WithTenant(TenantId).WithBranch(BranchId).Build());

        // Global VariationOptions (BranchId = null is allowed; LoadActiveGlobalOptionsAsync
        // filters by name + IsActive only).
        Db.VariationOptions.Add(new VariationOption
        {
            Id = OptSizeId,
            Title = "Size",
            Name = "Size",
            DisplayOrder = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            TenantId = TenantId,
        });
        if (seedColorOption)
        {
            Db.VariationOptions.Add(new VariationOption
            {
                Id = OptColorId,
                Title = "Color",
                Name = "Color",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                TenantId = TenantId,
            });
        }
        await Db.SaveChangesAsync();
    }

    private static AppDtos.ProductVariation.Request.VariationOptionDto OptionDto(string name, params string[] values) =>
        new()
        {
            Name = name,
            DisplayOrder = 0,
            Values = values.Select((v, i) => new AppDtos.ProductVariation.Request.VariationValueDto
            {
                Value = v,
                DisplayOrder = i,
            }).ToList(),
        };

    private static AppDtos.ProductVariation.Request.ProductVariantDto VariantDto(
        string sku,
        decimal price,
        decimal initialStock,
        params string[] valueKeys) =>
        new()
        {
            Sku = sku,
            Price = price,
            InitialStock = initialStock,
            IsActive = true,
            VariationValueKeys = valueKeys.ToList(),
        };

    [Fact]
    public async Task Two_options_two_values_each_creates_four_variants()
    {
        await SeedSchemaAsync();
        var sut = BuildHandler();

        var cmd = new CreateProductWithVariationsCommand(
            Title: "T-shirt",
            Code: "TS-1",
            ParentId: null,
            CategoryId: CategoryId,
            Price: 100m,
            InitialStock: 0m,
            IsActive: true,
            ImageUrl: null,
            ImageGallery: null,
            Description: null,
            Color: null,
            Warranty: null,
            VariationOptions: new()
            {
                OptionDto("Size", "S", "M"),
                OptionDto("Color", "Red", "Blue"),
            },
            ProductVariants: new()
            {
                VariantDto("TS-1-S-RED",  100m, 0m, "0:0", "1:0"),
                VariantDto("TS-1-S-BLUE", 100m, 0m, "0:0", "1:1"),
                VariantDto("TS-1-M-RED",  100m, 0m, "0:1", "1:0"),
                VariantDto("TS-1-M-BLUE", 100m, 0m, "0:1", "1:1"),
            });

        var productId = await sut.Handle(cmd, CancellationToken.None);

        productId.Should().BeGreaterThan(0);
        var variants = await Db.ProductVariants.AsNoTracking()
            .Where(v => v.ProductId == productId).ToListAsync();
        variants.Should().HaveCount(4);
        variants.Select(v => v.Sku).Should().BeEquivalentTo(
            new[] { "TS-1-S-RED", "TS-1-S-BLUE", "TS-1-M-RED", "TS-1-M-BLUE" });

        var product = await Db.Products.AsNoTracking().FirstAsync(p => p.Id == productId);
        product.HasVariations.Should().BeTrue();
    }

    // Regression test for the same duplicate-Stock-row bug as CreateProductHandler.
    // Fixed in StockService.GetTrackedStockByVariantAndBranchAsync by checking DbSet.Local
    // before falling back to a DB query.
    [Fact]
    public async Task With_initial_stock_per_variant_seeds_stock_and_movements()
    {
        await SeedSchemaAsync();
        var sut = BuildHandler();

        var cmd = new CreateProductWithVariationsCommand(
            Title: "Shirt",
            Code: "SH-1",
            ParentId: null,
            CategoryId: CategoryId,
            Price: 100m,
            InitialStock: 0m,
            IsActive: true,
            ImageUrl: null,
            ImageGallery: null,
            Description: null,
            Color: null,
            Warranty: null,
            VariationOptions: new()
            {
                OptionDto("Size", "S", "M"),
            },
            ProductVariants: new()
            {
                VariantDto("SH-1-S", 100m, 5m, "0:0"),
                VariantDto("SH-1-M", 100m, 7m, "0:1"),
            });

        var productId = await sut.Handle(cmd, CancellationToken.None);

        var variants = await Db.ProductVariants.AsNoTracking()
            .Where(v => v.ProductId == productId).ToListAsync();
        variants.Should().HaveCount(2);

        var stocks = await Db.Stocks.AsNoTracking()
            .Where(s => variants.Select(v => v.Id).Contains(s.ProductVariantId))
            .ToListAsync();
        stocks.Should().HaveCount(2);
        stocks.Sum(s => s.AvailableQuantity).Should().Be(12m);

        var movements = await Db.StockMovements.AsNoTracking().ToListAsync();
        movements.Should().HaveCount(2);
    }

    [Fact]
    public async Task Single_option_three_values_creates_three_variants()
    {
        await SeedSchemaAsync(seedColorOption: false);
        var sut = BuildHandler();

        var cmd = new CreateProductWithVariationsCommand(
            Title: "Mug",
            Code: "MUG-1",
            ParentId: null,
            CategoryId: CategoryId,
            Price: 50m,
            InitialStock: 0m,
            IsActive: true,
            ImageUrl: null,
            ImageGallery: null,
            Description: null,
            Color: null,
            Warranty: null,
            VariationOptions: new()
            {
                OptionDto("Size", "S", "M", "L"),
            },
            ProductVariants: new()
            {
                VariantDto("MUG-1-S", 50m, 0m, "0:0"),
                VariantDto("MUG-1-M", 50m, 0m, "0:1"),
                VariantDto("MUG-1-L", 50m, 0m, "0:2"),
            });

        var productId = await sut.Handle(cmd, CancellationToken.None);

        var variants = await Db.ProductVariants.AsNoTracking()
            .Where(v => v.ProductId == productId).ToListAsync();
        variants.Should().HaveCount(3);
    }

    [Fact]
    public async Task Variant_title_built_from_combination_of_variation_values()
    {
        await SeedSchemaAsync();
        var sut = BuildHandler();

        var cmd = new CreateProductWithVariationsCommand(
            Title: "Hat",
            Code: "HAT-1",
            ParentId: null,
            CategoryId: CategoryId,
            Price: 30m,
            InitialStock: 0m,
            IsActive: true,
            ImageUrl: null,
            ImageGallery: null,
            Description: null,
            Color: null,
            Warranty: null,
            VariationOptions: new()
            {
                OptionDto("Size", "S"),
                OptionDto("Color", "Red"),
            },
            ProductVariants: new()
            {
                VariantDto("HAT-1-S-RED", 30m, 0m, "0:0", "1:0"),
            });

        var productId = await sut.Handle(cmd, CancellationToken.None);

        var variant = await Db.ProductVariants.AsNoTracking()
            .FirstAsync(v => v.ProductId == productId);
        variant.Sku.Should().Be("HAT-1-S-RED");
        // BuildDisplayTitle joins with " / " in option order.
        variant.Title.Should().Be("S / Red");
        variant.Name.Should().Be("S / Red");
    }
}

