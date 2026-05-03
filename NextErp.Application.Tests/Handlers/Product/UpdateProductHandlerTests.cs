using AutoMapper;
using NextErp.Application.Commands;
using NextErp.Application.DTOs;
using NextErp.Application.Handlers.CommandHandlers.Product;
using NextErp.Application.Services;

namespace NextErp.Application.Tests.Handlers.Product;

public class UpdateProductHandlerTests : HandlerTestBase
{
    private static readonly IMapper Mapper = BuildMapper();

    private static IMapper BuildMapper()
    {
        var cfg = new MapperConfiguration(c =>
            c.AddMaps(typeof(NextErp.Application.ApplicationAssemblyMarker).Assembly));
        return cfg.CreateMapper();
    }

    private UpdateProductHandler BuildHandler()
    {
        var stockService = new StockService(Db, BranchProvider);
        return new UpdateProductHandler(Db, stockService, Mapper);
    }

    private const int ProductId = 100;
    private const int CategoryIdA = 100;
    private const int CategoryIdB = 101;

    private async Task SeedAsync(Guid? productBranchOverride = null)
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder()
            .WithId(CategoryIdA).WithTitle("Cat A").WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Categories.Add(new CategoryBuilder()
            .WithId(CategoryIdB).WithTitle("Cat B").WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(ProductId)
            .WithTitle("Original Title")
            .WithCode("ORIG-001")
            .WithPrice(100m)
            .WithCategory(CategoryIdA)
            .WithTenant(TenantId)
            .WithBranch(productBranchOverride ?? BranchId)
            .Build());
        await Db.SaveChangesAsync();
    }

    private static UpdateProductCommand BuildUpdateCommand(
        int id = ProductId,
        string title = "Updated Title",
        decimal price = 250m,
        int categoryId = CategoryIdB,
        IReadOnlyList<DTOs.Product.Request.GalleryResolvedSlot>? gallery = null)
        => new(
            Id: id,
            Title: title,
            Code: "ORIG-001",
            ParentId: null,
            CategoryId: categoryId,
            Price: price,
            IsActive: true,
            ImageUrl: null,
            ImageGallery: gallery);

    [Fact]
    public async Task Happy_path_updates_title_price_category_and_flushes()
    {
        await SeedAsync();
        var sut = BuildHandler();

        await sut.Handle(BuildUpdateCommand(), CancellationToken.None);

        var fresh = await Db.Products.AsNoTracking().FirstAsync(p => p.Id == ProductId);
        fresh.Title.Should().Be("Updated Title");
        fresh.Price.Should().Be(250m);
        fresh.CategoryId.Should().Be(CategoryIdB);
    }

    [Fact]
    public async Task Not_found_throws_KeyNotFoundException()
    {
        await SeedAsync();
        var sut = BuildHandler();

        var cmd = BuildUpdateCommand(id: 9999);
        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        (await act.Should().ThrowAsync<KeyNotFoundException>())
            .WithMessage("*9999*");
    }

    [Fact]
    public async Task UpdatedAt_is_set_to_recent_UtcNow()
    {
        await SeedAsync();
        var before = DateTime.UtcNow;
        var sut = BuildHandler();

        await sut.Handle(BuildUpdateCommand(), CancellationToken.None);

        var fresh = await Db.Products.AsNoTracking().FirstAsync(p => p.Id == ProductId);
        fresh.UpdatedAt.Should().NotBeNull();
        fresh.UpdatedAt!.Value.Should().BeOnOrAfter(before).And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Branch_scoped_other_branch_product_not_found()
    {
        // Seed product in a different branch — the global filter on [BranchScoped] Product
        // strips it from dbContext.Products, so the handler treats it as not found.
        var otherBranchId = Guid.NewGuid();
        Db.Branches.Add(new BranchBuilder().WithId(otherBranchId).WithTenant(TenantId).Build());
        await SeedAsync(productBranchOverride: otherBranchId);
        var sut = BuildHandler();

        var act = async () => await sut.Handle(BuildUpdateCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ImageGallery_replaces_existing_product_images()
    {
        await SeedAsync();
        // Seed two existing image rows so we can confirm they're removed and replaced.
        Db.ProductImages.Add(new NextErp.Domain.Entities.ProductImage
        {
            ProductId = ProductId,
            Url = "https://old/1.jpg",
            DisplayOrder = 0,
            IsThumbnail = true,
            Title = "",
            CreatedAt = DateTime.UtcNow,
        });
        Db.ProductImages.Add(new NextErp.Domain.Entities.ProductImage
        {
            ProductId = ProductId,
            Url = "https://old/2.jpg",
            DisplayOrder = 1,
            IsThumbnail = false,
            Title = "",
            CreatedAt = DateTime.UtcNow,
        });
        await Db.SaveChangesAsync();

        var gallery = new List<DTOs.Product.Request.GalleryResolvedSlot>
        {
            new("https://new/a.jpg", IsThumbnail: true),
            new("https://new/b.jpg", IsThumbnail: false),
        };
        var sut = BuildHandler();

        await sut.Handle(BuildUpdateCommand(gallery: gallery), CancellationToken.None);

        var images = await Db.ProductImages.AsNoTracking()
            .Where(pi => pi.ProductId == ProductId)
            .OrderBy(pi => pi.DisplayOrder)
            .ToListAsync();
        images.Should().HaveCount(2);
        images.Select(i => i.Url).Should().BeEquivalentTo(new[] { "https://new/a.jpg", "https://new/b.jpg" });

        var fresh = await Db.Products.AsNoTracking().FirstAsync(p => p.Id == ProductId);
        fresh.ImageUrl.Should().Be("https://new/a.jpg");
    }
}

