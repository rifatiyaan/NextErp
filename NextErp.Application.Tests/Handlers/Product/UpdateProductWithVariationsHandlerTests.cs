using AutoMapper;
using AppDtos = NextErp.Application.DTOs;
using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Product;
using NextErp.Application.Services;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Product;

public class UpdateProductWithVariationsHandlerTests : HandlerTestBase
{
    private static readonly IMapper Mapper = BuildMapper();

    private static IMapper BuildMapper()
    {
        var cfg = new MapperConfiguration(c =>
            c.AddMaps(typeof(NextErp.Application.ApplicationAssemblyMarker).Assembly));
        return cfg.CreateMapper();
    }

    private const int CategoryId = 500;
    private const int ProductId = 500;
    private const int OptSizeId = 500;
    private const int OptColorId = 501;

    // Existing variants (seeded before update).
    private const int VariantSRedId = 510;
    private const int VariantSBlueId = 511;

    // Existing variation values (matched on Id by ConfigurableProductVariantFactory).
    private const int ValSId = 520;
    private const int ValMId = 521;
    private const int ValRedId = 522;
    private const int ValBlueId = 523;

    private UpdateProductWithVariationsHandler BuildHandler()
    {
        var stockService = new StockService(Db, BranchProvider);
        return new UpdateProductWithVariationsHandler(Db, stockService, Mapper);
    }

    private async Task SeedAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder()
            .WithId(CategoryId).WithTitle("Cat").WithTenant(TenantId).WithBranch(BranchId).Build());

        // Global VariationOptions (Size + Color).
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

        // Variation values in the catalog.
        Db.VariationValues.Add(new VariationValue
        {
            Id = ValSId, Title = "S", Name = "S", Value = "S",
            VariationOptionId = OptSizeId, DisplayOrder = 0, IsActive = true,
            CreatedAt = DateTime.UtcNow, TenantId = TenantId,
        });
        Db.VariationValues.Add(new VariationValue
        {
            Id = ValMId, Title = "M", Name = "M", Value = "M",
            VariationOptionId = OptSizeId, DisplayOrder = 1, IsActive = true,
            CreatedAt = DateTime.UtcNow, TenantId = TenantId,
        });
        Db.VariationValues.Add(new VariationValue
        {
            Id = ValRedId, Title = "Red", Name = "Red", Value = "Red",
            VariationOptionId = OptColorId, DisplayOrder = 0, IsActive = true,
            CreatedAt = DateTime.UtcNow, TenantId = TenantId,
        });
        Db.VariationValues.Add(new VariationValue
        {
            Id = ValBlueId, Title = "Blue", Name = "Blue", Value = "Blue",
            VariationOptionId = OptColorId, DisplayOrder = 1, IsActive = true,
            CreatedAt = DateTime.UtcNow, TenantId = TenantId,
        });

        var product = new ProductBuilder()
            .WithId(ProductId)
            .WithTitle("Original")
            .WithCode("ORIG-1")
            .WithCategory(CategoryId)
            .WithPrice(100m)
            .WithTenant(TenantId)
            .WithBranch(BranchId)
            .WithVariations()
            .Build();
        Db.Products.Add(product);

        // Bind both options to the product.
        Db.ProductVariationOptions.Add(new ProductVariationOption
        {
            Title = "Size",
            ProductId = ProductId,
            VariationOptionId = OptSizeId,
            DisplayOrder = 0,
            CreatedAt = DateTime.UtcNow,
        });
        Db.ProductVariationOptions.Add(new ProductVariationOption
        {
            Title = "Color",
            ProductId = ProductId,
            VariationOptionId = OptColorId,
            DisplayOrder = 1,
            CreatedAt = DateTime.UtcNow,
        });

        // Two existing variants: S/Red and S/Blue.
        var variantSRed = new ProductVariantBuilder()
            .WithId(VariantSRedId)
            .WithProduct(ProductId)
            .WithSku("ORIG-S-RED")
            .WithTitle("S / Red")
            .WithTenant(TenantId)
            .WithBranch(BranchId)
            .Build();
        var variantSBlue = new ProductVariantBuilder()
            .WithId(VariantSBlueId)
            .WithProduct(ProductId)
            .WithSku("ORIG-S-BLUE")
            .WithTitle("S / Blue")
            .WithTenant(TenantId)
            .WithBranch(BranchId)
            .Build();

        Db.ProductVariants.Add(variantSRed);
        Db.ProductVariants.Add(variantSBlue);
        await Db.SaveChangesAsync();

        // Bind variation values to existing variants (many-to-many).
        var sRed = await Db.ProductVariants.FirstAsync(v => v.Id == VariantSRedId);
        sRed.VariationValues.Add(await Db.VariationValues.FirstAsync(v => v.Id == ValSId));
        sRed.VariationValues.Add(await Db.VariationValues.FirstAsync(v => v.Id == ValRedId));
        var sBlue = await Db.ProductVariants.FirstAsync(v => v.Id == VariantSBlueId);
        sBlue.VariationValues.Add(await Db.VariationValues.FirstAsync(v => v.Id == ValSId));
        sBlue.VariationValues.Add(await Db.VariationValues.FirstAsync(v => v.Id == ValBlueId));
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
        params string[] valueKeys) =>
        new()
        {
            Sku = sku,
            Price = price,
            InitialStock = 0m,
            IsActive = true,
            VariationValueKeys = valueKeys.ToList(),
        };

    private static UpdateProductWithVariationsCommand BuildCmd(
        List<AppDtos.ProductVariation.Request.VariationOptionDto> options,
        List<AppDtos.ProductVariation.Request.ProductVariantDto> variants,
        string title = "Updated") =>
        new(
            Id: ProductId,
            Title: title,
            Code: "ORIG-1",
            ParentId: null,
            CategoryId: CategoryId,
            Price: 100m,
            IsActive: true,
            ImageUrl: null,
            ImageGallery: null,
            ImageThumbnailUpdates: null,
            Description: null,
            Color: null,
            Warranty: null,
            VariationOptions: options,
            ProductVariants: variants);

    [Fact]
    public async Task Modify_titles_only_existing_variants_stay_active_no_new_inserts()
    {
        await SeedAsync();
        var sut = BuildHandler();

        var cmd = BuildCmd(
            options: new()
            {
                OptionDto("Size", "S", "M"),
                OptionDto("Color", "Red", "Blue"),
            },
            variants: new()
            {
                // Same combinations as seeded, just with different SKU prices.
                VariantDto("ORIG-S-RED",  150m, "0:0", "1:0"),
                VariantDto("ORIG-S-BLUE", 175m, "0:0", "1:1"),
            },
            title: "Updated Title");

        await sut.Handle(cmd, CancellationToken.None);

        var variants = await Db.ProductVariants.AsNoTracking()
            .Where(v => v.ProductId == ProductId)
            .ToListAsync();
        variants.Should().HaveCount(2);
        variants.Should().AllSatisfy(v => v.IsActive.Should().BeTrue());

        // Prices were updated in-place; no new variant rows added.
        variants.Single(v => v.Id == VariantSRedId).Price.Should().Be(150m);
        variants.Single(v => v.Id == VariantSBlueId).Price.Should().Be(175m);

        // Product title flowed via mapper.
        var product = await Db.Products.AsNoTracking().FirstAsync(p => p.Id == ProductId);
        product.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task Adding_a_new_variant_combination_inserts_a_new_ProductVariant()
    {
        await SeedAsync();
        var sut = BuildHandler();

        var cmd = BuildCmd(
            options: new()
            {
                OptionDto("Size", "S", "M"),
                OptionDto("Color", "Red", "Blue"),
            },
            variants: new()
            {
                VariantDto("ORIG-S-RED",  100m, "0:0", "1:0"),
                VariantDto("ORIG-S-BLUE", 100m, "0:0", "1:1"),
                // New combination M/Red.
                VariantDto("ORIG-M-RED",  120m, "0:1", "1:0"),
            });

        await sut.Handle(cmd, CancellationToken.None);

        var variants = await Db.ProductVariants.AsNoTracking()
            .Where(v => v.ProductId == ProductId)
            .ToListAsync();
        variants.Should().HaveCount(3);
        variants.Should().Contain(v => v.Sku == "ORIG-M-RED" && v.Price == 120m && v.IsActive);
    }

    [Fact]
    public async Task Removing_a_variant_from_request_marks_existing_inactive_not_deleted()
    {
        await SeedAsync();
        var sut = BuildHandler();

        var cmd = BuildCmd(
            options: new()
            {
                OptionDto("Size", "S", "M"),
                OptionDto("Color", "Red", "Blue"),
            },
            variants: new()
            {
                // Drop S/Blue from the request — handler should soft-deactivate, not delete.
                VariantDto("ORIG-S-RED", 100m, "0:0", "1:0"),
            });

        await sut.Handle(cmd, CancellationToken.None);

        var variants = await Db.ProductVariants.AsNoTracking()
            .Where(v => v.ProductId == ProductId)
            .ToListAsync();
        variants.Should().HaveCount(2); // both rows still present in DB
        variants.Single(v => v.Id == VariantSRedId).IsActive.Should().BeTrue();
        variants.Single(v => v.Id == VariantSBlueId).IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Empty_variants_list_throws_at_least_one_required()
    {
        await SeedAsync();
        var sut = BuildHandler();

        var cmd = BuildCmd(
            options: new()
            {
                OptionDto("Size", "S", "M"),
            },
            variants: new()); // empty

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*at least one*");
    }
}

