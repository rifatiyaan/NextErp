using FluentAssertions;
using FluentValidation;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Handlers.CommandHandlers.Ecommerce;
using NextErp.Application.Handlers.QueryHandlers.Ecommerce;
using NextErp.Application.Queries.Ecommerce;
using NextErp.Application.Tests.Builders;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;
using ProductEntity = NextErp.Domain.Entities.Product;

namespace NextErp.Application.Tests.Handlers.Ecommerce;

public class ReviewHandlerTests : HandlerTestBase
{
    private const int ProductId = 9100;

    // Product -> Category/Branch are real FKs, so the chain must exist first.
    private ProductEntity SeedProduct(bool published = true)
    {
        if (!Db.Branches.Local.Any(b => b.Id == BranchId) && !Db.Branches.Any(b => b.Id == BranchId))
            Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());

        Db.Categories.Add(new CategoryBuilder().WithId(910).WithTitle("Cat").WithTenant(TenantId).WithBranch(BranchId).Build());
        var product = new ProductBuilder().WithId(ProductId).WithTitle("Widget").WithCode("P910001")
            .WithPrice(100m).WithCategory(910).WithTenant(TenantId).WithBranch(BranchId).Build();
        product.IsActive = true;
        product.IsPublishedOnline = published;
        Db.Products.Add(product);
        Db.SaveChanges();
        return product;
    }

    [Fact]
    public async Task CreateReview_stores_an_approved_trimmed_review_for_a_published_product()
    {
        var product = SeedProduct();
        var sut = new CreateReviewHandler(Db);

        var id = await sut.Handle(new CreateReviewCommand(product.Id, "  Amina  ", 5, "  Great kit  "), CancellationToken.None);

        id.Should().BeGreaterThan(0);
        var saved = Db.Reviews.Single(r => r.Id == id);
        saved.ProductId.Should().Be(product.Id);
        saved.AuthorName.Should().Be("Amina");
        saved.Text.Should().Be("Great kit");
        saved.Rating.Should().Be(5);
        saved.IsApproved.Should().BeTrue();
        saved.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public async Task CreateReview_rejects_an_unpublished_product()
    {
        SeedProduct(published: false);
        var sut = new CreateReviewHandler(Db);

        var act = () => sut.Handle(new CreateReviewCommand(ProductId, "Amina", 4, "Nice"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        Db.Reviews.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProductReviews_returns_only_approved_newest_first_with_average_and_count()
    {
        var product = SeedProduct();
        Db.Reviews.AddRange(
            new Review { ProductId = product.Id, AuthorName = "A", Rating = 5, Text = "x", IsApproved = true, TenantId = TenantId, CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new Review { ProductId = product.Id, AuthorName = "B", Rating = 3, Text = "y", IsApproved = true, TenantId = TenantId, CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
            new Review { ProductId = product.Id, AuthorName = "Hidden", Rating = 1, Text = "z", IsApproved = false, TenantId = TenantId, CreatedAt = DateTime.UtcNow });
        Db.SaveChanges();
        var sut = new GetProductReviewsHandler(Db);

        var result = await sut.Handle(new GetProductReviewsQuery(product.Id), CancellationToken.None);

        result.Count.Should().Be(2);
        result.Average.Should().Be(4.0);
        result.Items.Should().OnlyContain(i => i.AuthorName != "Hidden");
        result.Items.First().AuthorName.Should().Be("B");
    }

    [Fact]
    public async Task GetProductReviews_returns_empty_when_no_reviews()
    {
        var product = SeedProduct();
        var sut = new GetProductReviewsHandler(Db);

        var result = await sut.Handle(new GetProductReviewsQuery(product.Id), CancellationToken.None);

        result.Count.Should().Be(0);
        result.Average.Should().Be(0);
        result.Items.Should().BeEmpty();
    }
}
