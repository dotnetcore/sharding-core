using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sample.NoShardingMultiLevel.Entities;

namespace Sample.NoShardingMultiLevel.Maps
{
    public class BossMap:IEntityTypeConfiguration<Boss>
    {
        public void Configure(EntityTypeBuilder<Boss> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().HasMaxLength(50);
            builder.Property(o => o.Name).IsRequired().HasMaxLength(50);
            builder.HasOne(o => o.Company).WithOne(o=>o.Boss).HasForeignKey<Company>(o=>o.BossId);
            builder.ToTable(nameof(Boss));
        }
    }
}