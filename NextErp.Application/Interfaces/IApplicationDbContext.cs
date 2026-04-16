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

        // Inventory Module DbSets
        DbSet<Stock> Stocks { get; set; }
        DbSet<Purchase> Purchases { get; set; }
        DbSet<PurchaseItem> PurchaseItems { get; set; }
        DbSet<Sale> Sales { get; set; }
        DbSet<SaleItem> SaleItems { get; set; }
        DbSet<SalePayment> SalePayments { get; set; }
        DbSet<StockMovement> StockMovements { get; set; }
        DbSet<UnitOfMeasure> UnitOfMeasures { get; set; }

        // Variation System DbSets
        DbSet<VariationOption> VariationOptions { get; set; }
        DbSet<VariationValue> VariationValues { get; set; }
        DbSet<ProductVariationOption> ProductVariationOptions { get; set; }
        DbSet<ProductVariant> ProductVariants { get; set; }

        // Transaction Support
        DatabaseFacade Database { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
