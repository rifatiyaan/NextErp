using NextErp.Application.Handlers.QueryHandlers.Ecommerce;
using NextErp.Application.Queries.Ecommerce;
using NextErp.Application.Tests.Builders;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Ecommerce;

public class OnlineOrderQueryHandlersTests : HandlerTestBase
{
    // OnlineOrderItem.ProductVariantId is a real FK (Restrict) to ProductVariant, so the
    // referenced Branch/Category/Product/Variant chain must exist first — the brief's
    // snippet assumed a bare int would satisfy SQLite, but the schema enforces the FK
    // even in the in-memory test DB. Mirrors ConfirmOnlineOrderHandlerTests.SeedOrder().
    private void SeedOrders()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        var category = new CategoryBuilder().WithId(800).WithTitle("Cat").WithTenant(TenantId).WithBranch(BranchId).Build();
        Db.Categories.Add(category);
        var product = new ProductBuilder().WithId(8000).WithTitle("Widget").WithCode("P800001")
            .WithPrice(100m).WithCategory(800).WithTenant(TenantId).WithBranch(BranchId).Build();
        Db.Products.Add(product);
        var variant = SimpleProductVariantFactory.CreateDefault(product);
        variant.Id = 1;
        Db.ProductVariants.Add(variant);

        for (var i = 1; i <= 3; i++)
        {
            Db.OnlineOrders.Add(new OnlineOrder
            {
                OrderNumber = $"W00000{i}",
                CustomerName = $"Customer {i}",
                Phone = "017",
                Address = "A",
                Status = i == 3 ? OnlineOrderStatus.Cancelled : OnlineOrderStatus.Pending,
                DeliveryFee = 60m,
                TenantId = TenantId,
                BranchId = BranchId,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                Items = { new OnlineOrderItem { ProductVariantId = 1, ProductTitle = "T", Sku = "S", UnitPrice = 100m, Quantity = 1m, LineTotal = 100m } },
            });
        }
        Db.SaveChanges();
    }

    [Fact]
    public async Task Paged_list_filters_by_status_newest_first()
    {
        SeedOrders();
        var sut = new GetPagedOnlineOrdersHandler(Db);

        var page = await sut.Handle(new GetPagedOnlineOrdersQuery("Pending"), CancellationToken.None);

        page.Total.Should().Be(2);
        page.Data.Should().HaveCount(2);
        page.Data[0].OrderNumber.Should().Be("W000001"); // newest (CreatedAt -1 min)
        page.Data[0].ItemsTotal.Should().Be(100m);
    }

    [Fact]
    public async Task Detail_returns_items_and_null_for_missing()
    {
        SeedOrders();
        var sut = new GetOnlineOrderByIdHandler(Db);
        var id = Db.OnlineOrders.First().Id;

        var detail = await sut.Handle(new GetOnlineOrderByIdQuery(id), CancellationToken.None);
        detail!.Items.Should().ContainSingle();

        (await sut.Handle(new GetOnlineOrderByIdQuery(99999), CancellationToken.None)).Should().BeNull();
    }
}
