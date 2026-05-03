using NextErp.Application.Handlers.QueryHandlers.Sale;
using NextErp.Application.Queries;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Sale;

public class GetSaleByIdHandlerTests : HandlerTestBase
{
    private GetSaleByIdHandler BuildHandler() => new(Db);

    [Fact]
    public async Task Existing_sale_returned_with_items_and_payments()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder()
            .WithId(1000).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(1000).WithCategory(1000).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(1000).WithProduct(1000).WithSku("S-V-1000")
            .WithTenant(TenantId).WithBranch(BranchId).Build());

        var saleId = Guid.NewGuid();
        var sale = new SaleBuilder()
            .WithId(saleId).WithTotalAmount(200m).WithFinalAmount(200m)
            .WithTenant(TenantId).WithBranch(BranchId).Build();
        sale.Items.Add(new SaleItem
        {
            Id = Guid.NewGuid(),
            SaleId = saleId,
            Title = "Line",
            ProductVariantId = 1000,
            Quantity = 2m,
            Price = 100m,
            CreatedAt = DateTime.UtcNow,
            TenantId = TenantId,
        });
        sale.Payments.Add(new SalePayment
        {
            Id = Guid.NewGuid(),
            SaleId = saleId,
            Title = "Cash",
            Amount = 200m,
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            TenantId = TenantId,
        });
        Db.Sales.Add(sale);
        await Db.SaveChangesAsync();

        var sut = BuildHandler();
        var result = await sut.Handle(new GetSaleByIdQuery(saleId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(saleId);
        result.Items.Should().HaveCount(1);
        result.Payments.Should().HaveCount(1);
    }
}

