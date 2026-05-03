using NextErp.Application.Handlers.QueryHandlers.Purchase;
using NextErp.Application.Queries;
using NextErp.Domain.Entities;
using PurchaseEntity = NextErp.Domain.Entities.Purchase;

namespace NextErp.Application.Tests.Handlers.Purchase;

public class GetPurchaseByIdHandlerTests : HandlerTestBase
{
    private GetPurchaseByIdHandler BuildHandler() => new(Db);

    [Fact]
    public async Task Existing_purchase_returned_with_items()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder()
            .WithId(1100).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(1100).WithCategory(1100).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(1100).WithProduct(1100).WithSku("PUR-V")
            .WithTenant(TenantId).WithBranch(BranchId).Build());

        var purchaseId = Guid.NewGuid();
        var purchase = new PurchaseEntity
        {
            Id = purchaseId,
            Title = "Test Purchase",
            PurchaseNumber = "PUR-XYZ",
            PurchaseDate = DateTime.UtcNow,
            TotalAmount = 50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            TenantId = TenantId,
            BranchId = BranchId,
        };
        purchase.Items.Add(new PurchaseItem
        {
            Id = Guid.NewGuid(),
            PurchaseId = purchaseId,
            Title = "Line",
            ProductVariantId = 1100,
            Quantity = 5m,
            UnitCost = 10m,
            CreatedAt = DateTime.UtcNow,
            TenantId = TenantId,
        });
        Db.Purchases.Add(purchase);
        await Db.SaveChangesAsync();

        var sut = BuildHandler();
        var result = await sut.Handle(new GetPurchaseByIdQuery(purchaseId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(purchaseId);
        result.Items.Should().HaveCount(1);
        result.Items.First().Quantity.Should().Be(5m);
    }
}

