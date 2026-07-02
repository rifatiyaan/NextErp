using NextErp.Application.Ecommerce;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Ecommerce;

public class OnlineOrderNumberFactoryTests : HandlerTestBase
{
    private OnlineOrder OrderWithNumber(string number) => new()
    {
        OrderNumber = number,
        CustomerName = "C",
        Phone = "017",
        Address = "A",
        TenantId = TenantId,
        BranchId = BranchId,
        CreatedAt = DateTime.UtcNow,
    };

    [Fact]
    public async Task First_number_is_W000001()
    {
        var number = await OnlineOrderNumberFactory.NextNumberAsync(TenantId, Db);
        number.Should().Be("W000001");
    }

    [Fact]
    public async Task Continues_from_existing_max()
    {
        Db.OnlineOrders.Add(OrderWithNumber("W000007"));
        await Db.SaveChangesAsync();

        var number = await OnlineOrderNumberFactory.NextNumberAsync(TenantId, Db);
        number.Should().Be("W000008");
    }
}
