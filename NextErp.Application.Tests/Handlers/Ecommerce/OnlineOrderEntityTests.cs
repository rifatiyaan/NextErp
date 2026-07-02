using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Ecommerce;

public class OnlineOrderEntityTests : HandlerTestBase
{
    [Fact]
    public async Task Online_order_with_items_persists_and_loads()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder()
            .WithId(1).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(1).WithCategory(1).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(1).WithProduct(1).WithSku("P000001a")
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();

        var order = new OnlineOrder
        {
            OrderNumber = "W000001",
            CustomerName = "Test Customer",
            Phone = "01700000000",
            Address = "12 Example Road, Dhaka",
            DeliveryFee = 60m,
            TenantId = TenantId,
            BranchId = BranchId,
            CreatedAt = DateTime.UtcNow,
            Items =
            {
                new OnlineOrderItem
                {
                    ProductVariantId = 1,
                    ProductTitle = "Sample",
                    Sku = "P000001a",
                    UnitPrice = 100m,
                    Quantity = 2m,
                    LineTotal = 200m,
                },
            },
        };
        Db.OnlineOrders.Add(order);
        await Db.SaveChangesAsync();

        var fresh = await Db.OnlineOrders.AsNoTracking()
            .Include(o => o.Items)
            .FirstAsync(o => o.OrderNumber == "W000001");
        fresh.Status.Should().Be(OnlineOrderStatus.Pending);
        fresh.Items.Should().ContainSingle(i => i.LineTotal == 200m);
    }
}
