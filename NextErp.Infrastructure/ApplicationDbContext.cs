using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Domain.Common;
using NextErp.Domain.Entities;
using NextErp.Infrastructure.Entities;
using NextErp.Infrastructure.Seeds;

namespace NextErp.Infrastructure
{
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IBranchProvider? branchProvider = null)
        : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IApplicationDbContext
    {
        private Guid? CurrentBranchId => branchProvider?.GetBranchId();
        private bool IsGlobalScope => branchProvider?.IsGlobal() ?? true;

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariation> ProductVariations { get; set; }
        public DbSet<VariationOption> VariationOptions { get; set; }
        public DbSet<VariationValue> VariationValues { get; set; }
        public DbSet<ProductVariationOption> ProductVariationOptions { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<NextErp.Domain.Entities.Module> Modules { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Party> Parties { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }

        // Inventory Module DbSets
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<SalePayment> SalePayments { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<UnitOfMeasure> UnitOfMeasures { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());

            builder.Entity<ApplicationUser>()
                .Property(u => u.BranchId)
                .IsRequired();

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Branch)
                .WithMany(b => b.Users)
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            ApplyBranchQueryFilters(builder);

            SeedData.SeedUnitsOfMeasure(builder);
        }

        private void ApplyBranchQueryFilters(ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var clr = entityType.ClrType;
                if (clr == null || entityType.IsOwned() || entityType.IsKeyless)
                    continue;

                if (!Attribute.IsDefined(clr, typeof(BranchScopedAttribute), inherit: false))
                    continue;

                const string branchPropertyName = "BranchId";
                var branchProp = clr.GetProperty(
                    branchPropertyName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (branchProp == null || branchProp.PropertyType != typeof(Guid))
                {
                    throw new InvalidOperationException(
                        $"[BranchScoped] type '{clr.Name}' must declare a public Guid {branchPropertyName} property.");
                }

                const string activePropertyName = "IsActive";
                var activeProp = clr.GetProperty(
                    activePropertyName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (activeProp == null || activeProp.PropertyType != typeof(bool))
                {
                    throw new InvalidOperationException(
                        $"[BranchScoped] type '{clr.Name}' must declare a public bool {activePropertyName} property for soft-delete filtering.");
                }

                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetBranchFilter), BindingFlags.Instance | BindingFlags.NonPublic)?
                    .MakeGenericMethod(clr);

                method?.Invoke(this, new object[] { builder });
            }
        }

        private void SetBranchFilter<TEntity>(ModelBuilder builder)
            where TEntity : class
        {
            Expression<Func<TEntity, bool>> filter = e =>
                IsGlobalScope
                || (CurrentBranchId.HasValue
                    && EF.Property<Guid>(e, "BranchId") == CurrentBranchId.Value
                    && EF.Property<bool>(e, "IsActive"));

            builder.Entity<TEntity>().HasQueryFilter(filter);
        }
    }
}