using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations
{
    public class VariationOptionConfiguration : IEntityTypeConfiguration<VariationOption>
    {
        public void Configure(EntityTypeBuilder<VariationOption> builder)
        {
            builder.HasIndex(vo => vo.Name);
        }
    }
}
