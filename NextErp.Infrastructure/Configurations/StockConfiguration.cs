using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NextErp.Infrastructure.Configurations
{
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .ValueGeneratedNever();

            builder.HasOne(s => s.ProductVariant)
                .WithOne(pv => pv.StockRecord)
                .HasForeignKey<Stock>(s => s.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(s => s.RowVersion)
                .IsRowVersion()
                .IsRequired();

            builder.Property(s => s.AvailableQuantity)
                .HasPrecision(18, 2);
        }
    }
}
