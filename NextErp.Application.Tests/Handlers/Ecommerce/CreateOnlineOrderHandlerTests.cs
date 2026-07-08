using FluentValidation;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Common.Settings;
using NextErp.Application.DTOs.Ecommerce;
using NextErp.Application.Handlers.CommandHandlers.Ecommerce;
using NextErp.Application.Settings;
using NextErp.Application.Tests.Builders;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;
using NSubstitute;

namespace NextErp.Application.Tests.Handlers.Ecommerce;

public class CreateOnlineOrderHandlerTests : HandlerTestBase
{
    private const int Cat = 800;
    private int _variantId;

    private ISettingsProvider Settings()
    {
        var provider = Substitute.For<ISettingsProvider>();
        provider.GetAsync<EcommerceSettings>().Returns(Task.FromResult(new EcommerceSettings
        {
            StorefrontEnabled = true,
            SellingBranchId = BranchId.ToString(),
            DeliveryFee = 60m,
        }));
        return provider;
    }

    private CreateOnlineOrderHandler BuildHandler() => new(Db, Settings(), Notifications);

    private async Task SeedPublishedProductAsync(bool published = true)
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        var cat = new CategoryBuilder().WithId(Cat).WithTitle("Cat").WithTenant(TenantId).WithBranch(BranchId).Build();
        cat.IsPublishedOnline = true;
        Db.Categories.Add(cat);
        var product = new ProductBuilder().WithId(8000).WithTitle("Widget").WithCode("P800001").WithPrice(150m)
            .WithCategory(Cat).WithTenant(TenantId).WithBranch(BranchId).Build();
        product.IsPublishedOnline = published;
        Db.Products.Add(product);
        var variant = SimpleProductVariantFactory.CreateDefault(product);
        variant.TenantId = TenantId;
        variant.BranchId = BranchId;
        Db.ProductVariants.Add(variant);
        await Db.SaveChangesAsync();
        _variantId = variant.Id;
    }

    [Fact]
    public async Task Creates_pending_order_with_snapshots_and_number()
    {
        await SeedPublishedProductAsync();
        var sut = BuildHandler();

        var orderNumber = await sut.Handle(new CreateOnlineOrderCommand(
            "Test Customer", "01700000000", "12 Example Road", null,
            new() { new StoreOrderItemRequest(_variantId, 2m) }), CancellationToken.None);

        orderNumber.Should().Be("W000001");
        var order = await Db.OnlineOrders.AsNoTracking().Include(o => o.Items).FirstAsync();
        order.Status.Should().Be(OnlineOrderStatus.Pending);
        order.DeliveryFee.Should().Be(60m);
        order.BranchId.Should().Be(BranchId);
        order.Items.Should().ContainSingle();
        order.Items.First().UnitPrice.Should().Be(150m);   // server-side snapshot, not client input
        order.Items.First().LineTotal.Should().Be(300m);
        order.Items.First().Sku.Should().Be("P800001-DEFAULT");
    }

    [Fact]
    public async Task Creates_order_when_selling_branch_id_is_garbage()
    {
        // Regression: checkout must not depend on a valid SellingBranchId. With
        // branch selling off (default) a blank/garbage value auto-resolves to the
        // default branch instead of throwing "storefront not available".
        await SeedPublishedProductAsync();
        var provider = Substitute.For<ISettingsProvider>();
        provider.GetAsync<EcommerceSettings>().Returns(Task.FromResult(new EcommerceSettings
        {
            StorefrontEnabled = true,
            EnableBranchSelling = false,
            SellingBranchId = "1", // not a GUID
            DeliveryFee = 60m,
        }));
        var sut = new CreateOnlineOrderHandler(Db, provider, Notifications);

        var orderNumber = await sut.Handle(new CreateOnlineOrderCommand(
            "Test Customer", "01700000000", "12 Example Road", null,
            new() { new StoreOrderItemRequest(_variantId, 1m) }), CancellationToken.None);

        orderNumber.Should().Be("W000001");
        var order = await Db.OnlineOrders.AsNoTracking().FirstAsync();
        order.BranchId.Should().Be(BranchId); // auto-resolved default branch
    }

    [Fact]
    public async Task Rejects_unpublished_product()
    {
        await SeedPublishedProductAsync(published: false);
        var sut = BuildHandler();

        var act = async () => await sut.Handle(new CreateOnlineOrderCommand(
            "Test Customer", "01700000000", "12 Example Road", null,
            new() { new StoreOrderItemRequest(_variantId, 1m) }), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Records_notification_for_staff()
    {
        await SeedPublishedProductAsync();
        var sut = BuildHandler();

        await sut.Handle(new CreateOnlineOrderCommand(
            "Test Customer", "01700000000", "12 Example Road", null,
            new() { new StoreOrderItemRequest(_variantId, 1m) }), CancellationToken.None);

        (await Db.Notifications.AsNoTracking().ToListAsync())
            .Should().ContainSingle(n => n.Type == "OnlineOrderPlaced");
    }

    [Fact]
    public async Task Notification_carries_the_final_order_number()
    {
        // Guards the ordering fix: the notification must be staged with the
        // order number that actually got persisted, not a candidate that a
        // concurrent checkout could have invalidated. The race itself isn't
        // reproducible single-threaded (see OnlineOrderNumberFactory retry
        // in CreateOnlineOrderHandler) — this asserts the normal path keeps
        // the notification and order number in sync.
        await SeedPublishedProductAsync();
        var sut = BuildHandler();

        var orderNumber = await sut.Handle(new CreateOnlineOrderCommand(
            "Test Customer", "01700000000", "12 Example Road", null,
            new() { new StoreOrderItemRequest(_variantId, 1m) }), CancellationToken.None);

        var order = await Db.OnlineOrders.AsNoTracking().FirstAsync();
        order.OrderNumber.Should().Be(orderNumber);

        var notification = (await Db.Notifications.AsNoTracking().ToListAsync())
            .Should().ContainSingle(n => n.Type == "OnlineOrderPlaced").Subject;
        notification.Message.Should().Contain(order.OrderNumber);
    }
}
