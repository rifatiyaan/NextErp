using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NextErp.Domain.Entities;

namespace NextErp.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Product> Products { get; set; }
        DbSet<Category> Categories { get; set; }
        DbSet<Module> Modules { get; set; }

        // Inventory Module DbSets
        DbSet<Supplier> Suppliers { get; set; }
        DbSet<Customer> Customers { get; set; }
        DbSet<Stock> Stocks { get; set; }
        DbSet<Purchase> Purchases { get; set; }
        DbSet<PurchaseItem> PurchaseItems { get; set; }
        DbSet<Sale> Sales { get; set; }
        DbSet<SaleItem> SaleItems { get; set; }

        // Variation System DbSets
        DbSet<VariationOption> VariationOptions { get; set; }
        DbSet<VariationValue> VariationValues { get; set; }
        DbSet<ProductVariant> ProductVariants { get; set; }

        // Transaction Support
        DatabaseFacade Database { get; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
