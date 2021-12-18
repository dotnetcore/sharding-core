using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sample.NoShardingMultiLevel.Entities;

namespace Sample.NoShardingMultiLevel.Maps
{
    public class CompanyMap : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {

            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().HasMaxLength(50);
            builder.Property(o => o.Name).IsRequired().HasMaxLength(50);
            builder.Property(o => o.BossId).IsRequired().HasMaxLength(50);
            builder.HasOne(o => o.Boss).WithOne(o => o.Company).HasForeignKey<Company>(o => o.BossId);
            builder.HasMany(o => o.Departments).WithOne(o => o.Company).HasForeignKey(o=>o.CompanyId);
            builder.ToTable(nameof(Company));
        }
    }
}