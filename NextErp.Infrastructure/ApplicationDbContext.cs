using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NextErp.Infrastructure.Entities;
using System.Linq.Expressions;

namespace NextErp.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
    {
        private readonly IBranchProvider? _branchProvider;
        private Guid? CurrentBranchId => _branchProvider?.GetBranchId();
        private bool IsGlobalScope => _branchProvider?.IsGlobal() ?? true;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IBranchProvider? branchProvider = null)
            : base(options)
        {
            _branchProvider = branchProvider;
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariation> ProductVariations { get; set; }
        public DbSet<VariationOption> VariationOptions { get; set; }
        public DbSet<VariationValue> VariationValues { get; set; }
        public DbSet<ProductVariationOption> ProductVariationOptions { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Party> Parties { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        // Inventory Module DbSets
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<SalePayment> SalePayments { get; set; }

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
        }

        private void ApplyBranchQueryFilters(ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (!typeof(IBranchEntity).IsAssignableFrom(entityType.ClrType))
                    continue;

                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetBranchFilter), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?
                    .MakeGenericMethod(entityType.ClrType);

                method?.Invoke(this, new object[] { builder });
            }
        }

        private void SetBranchFilter<TEntity>(ModelBuilder builder)
            where TEntity : class, IBranchEntity
        {
            Expression<Func<TEntity, bool>> filter = entity =>
                IsGlobalScope || (CurrentBranchId.HasValue && entity.BranchId == CurrentBranchId.Value);
            builder.Entity<TEntity>().HasQueryFilter(filter);
        }
    }
}