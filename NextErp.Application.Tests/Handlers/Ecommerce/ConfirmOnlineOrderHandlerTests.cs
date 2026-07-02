using MediatR;
using NextErp.Application.Commands;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Handlers.CommandHandlers.Ecommerce;
using NextErp.Application.Tests.Builders;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;
using NSubstitute;

namespace NextErp.Application.Tests.Handlers.Ecommerce;

public class ConfirmOnlineOrderHandlerTests : HandlerTestBase
{
    private const int VariantId = 42;

    // OnlineOrderItem.ProductVariantId is a real FK (Restrict) to ProductVariant,
    // so the referenced Branch/Category/Product/Variant chain must exist first —
    // the brief's snippet assumed a bare int would satisfy SQLite, but the schema
    // enforces the foreign key even in the in-memory test DB.
    private OnlineOrder SeedOrder(string phone = "01700000000")
    {
        if (!Db.Branches.Local.Any(b => b.Id == BranchId) && !Db.Branches.Any(b => b.Id == BranchId))
            Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());

        var category = new CategoryBuilder().WithId(800).WithTitle("Cat").WithTenant(TenantId).WithBranch(BranchId).Build();
        Db.Categories.Add(category);
        var product = new ProductBuilder().WithId(8000).WithTitle("Widget").WithCode("P800001")
            .WithPrice(150m).WithCategory(800).WithTenant(TenantId).WithBranch(BranchId).Build();
        Db.Products.Add(product);
        var variant = SimpleProductVariantFactory.CreateDefault(product);
        variant.Id = VariantId;
        Db.ProductVariants.Add(variant);

        var order = new OnlineOrder
        {
            OrderNumber = "W000001",
            CustomerName = "Test Customer",
            Phone = phone,
            Address = "12 Example Road",
            DeliveryFee = 60m,
            TenantId = TenantId,
            BranchId = BranchId,
            CreatedAt = DateTime.UtcNow,
            Items =
            {
                new OnlineOrderItem
                {
                    ProductVariantId = VariantId, ProductTitle = "Widget", Sku = "P800001a",
                    UnitPrice = 150m, Quantity = 2m, LineTotal = 300m,
                },
            },
        };
        Db.OnlineOrders.Add(order);
        Db.SaveChanges();
        return order;
    }

    // OnlineOrder.SaleId is a real FK (Restrict) to Sale, so even though IMediator
    // is substituted (the real Sale pipeline is deliberately not exercised — it has
    // its own tests), the in-memory SQLite DB still enforces referential integrity.
    // Stub a minimal Sale row with the mocked Id when the command is sent, standing
    // in for what CreateSaleHandler would have persisted.
    private (ConfirmOnlineOrderHandler sut, IMediator mediator) BuildHandler(Guid? saleId = null)
    {
        var resolvedSaleId = saleId ?? Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateSaleCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                Db.Sales.Add(new SaleBuilder().WithId(resolvedSaleId).WithTenant(TenantId).WithBranch(BranchId).Build());
                Db.SaveChanges();
                return Task.FromResult(resolvedSaleId);
            });
        return (new ConfirmOnlineOrderHandler(Db, mediator, BranchProvider), mediator);
    }

    [Fact]
    public async Task Confirm_creates_party_sends_sale_with_snapshot_prices_and_links()
    {
        var order = SeedOrder();
        var expectedSaleId = Guid.NewGuid();
        var (sut, mediator) = BuildHandler(expectedSaleId);

        var saleId = await sut.Handle(new ConfirmOnlineOrderCommand(order.Id), CancellationToken.None);

        saleId.Should().Be(expectedSaleId);
        await mediator.Received(1).Send(
            Arg.Is<CreateSaleCommand>(c =>
                c.Items.Count == 1 &&
                c.Items[0].ProductVariantId == 42 &&
                c.Items[0].Price == 150m &&
                c.Items[0].Quantity == 2m &&
                c.Discount == 0m),
            Arg.Any<CancellationToken>());

        var fresh = await Db.OnlineOrders.AsNoTracking().FirstAsync(o => o.Id == order.Id);
        fresh.Status.Should().Be(OnlineOrderStatus.Confirmed);
        fresh.SaleId.Should().Be(expectedSaleId);
        fresh.PartyId.Should().NotBeNull();

        var party = await Db.Parties.AsNoTracking().FirstAsync(p => p.Id == fresh.PartyId);
        party.Phone.Should().Be("01700000000");
        party.PartyType.Should().Be(PartyType.Customer);
    }

    [Fact]
    public async Task Confirm_reuses_existing_party_matched_by_phone()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        var existing = new NextErp.Domain.Entities.Party
        {
            Id = Guid.NewGuid(), Title = "Existing", Phone = "01700000000",
            PartyType = PartyType.Customer, TenantId = TenantId, BranchId = BranchId,
            CreatedAt = DateTime.UtcNow,
        };
        Db.Parties.Add(existing);
        await Db.SaveChangesAsync();
        var order = SeedOrder();
        var (sut, _) = BuildHandler();

        await sut.Handle(new ConfirmOnlineOrderCommand(order.Id), CancellationToken.None);

        (await Db.OnlineOrders.AsNoTracking().FirstAsync(o => o.Id == order.Id))
            .PartyId.Should().Be(existing.Id);
        (await Db.Parties.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Confirm_rejects_non_pending_order()
    {
        var order = SeedOrder();
        order.Status = OnlineOrderStatus.Cancelled;
        Db.SaveChanges();
        var (sut, _) = BuildHandler();

        var act = async () => await sut.Handle(new ConfirmOnlineOrderCommand(order.Id), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Pending*");
    }

    [Fact]
    public async Task Cancel_requires_reason_and_sets_status()
    {
        var order = SeedOrder();
        var sut = new CancelOnlineOrderHandler(Db);

        await sut.Handle(new CancelOnlineOrderCommand(order.Id, "Customer unreachable"), CancellationToken.None);

        var fresh = await Db.OnlineOrders.AsNoTracking().FirstAsync(o => o.Id == order.Id);
        fresh.Status.Should().Be(OnlineOrderStatus.Cancelled);
        fresh.CancelReason.Should().Be("Customer unreachable");
    }
}
