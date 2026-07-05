using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NextErp.Domain.Entities;

namespace NextErp.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Product> Products { get; set; }
        DbSet<ProductImage> ProductImages { get; set; }
        DbSet<Category> Categories { get; set; }
        DbSet<Module> Modules { get; set; }
        DbSet<Branch> Branches { get; set; }
        DbSet<Party> Parties { get; set; }
        DbSet<RolePermission> RolePermissions { get; set; }
        DbSet<SystemSettings> SystemSettings { get; set; }
        DbSet<ModuleSetting> ModuleSettings { get; set; }
        DbSet<Notification> Notifications { get; set; }

        // Inventory Module DbSets
        DbSet<Stock> Stocks { get; set; }
        DbSet<Purchase> Purchases { get; set; }
        DbSet<PurchaseItem> PurchaseItems { get; set; }
        DbSet<PurchaseReturn> PurchaseReturns { get; set; }
        DbSet<PurchaseReturnItem> PurchaseReturnItems { get; set; }
        DbSet<Sale> Sales { get; set; }
        DbSet<SaleItem> SaleItems { get; set; }
        DbSet<SalePayment> SalePayments { get; set; }
        DbSet<SaleReturn> SaleReturns { get; set; }
        DbSet<SaleReturnItem> SaleReturnItems { get; set; }
        DbSet<OnlineOrder> OnlineOrders { get; set; }
        DbSet<OnlineOrderItem> OnlineOrderItems { get; set; }
        DbSet<Review> Reviews { get; set; }
        DbSet<StockMovement> StockMovements { get; set; }
        DbSet<StockBatch> StockBatches { get; set; }
        DbSet<UnitOfMeasure> UnitOfMeasures { get; set; }

        // Variation System DbSets
        DbSet<VariationOption> VariationOptions { get; set; }
        DbSet<VariationValue> VariationValues { get; set; }
        DbSet<ProductVariationOption> ProductVariationOptions { get; set; }
        DbSet<ProductVariant> ProductVariants { get; set; }

        // Accounting
        DbSet<Account> Accounts { get; set; }
        DbSet<JournalEntry> JournalEntries { get; set; }
        DbSet<JournalLine> JournalLines { get; set; }

        // Loyalty / membership
        DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }

        // Promotions / discounts
        DbSet<Promotion> Promotions { get; set; }

        // Transaction Support
        DatabaseFacade Database { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
