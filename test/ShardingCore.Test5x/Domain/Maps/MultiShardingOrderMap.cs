using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShardingCore.Test5x.Domain.Entities;

namespace ShardingCore.Test5x.Domain.Maps
{
    public class MultiShardingOrderMap:IEntityTypeConfiguration<MultiShardingOrder>
    {
        public void Configure(EntityTypeBuilder<MultiShardingOrder> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).ValueGeneratedNever();
            builder.Property(o => o.Name).IsRequired().IsUnicode(false).HasMaxLength(50);
            builder.ToTable(nameof(MultiShardingOrder));
        }
    }
}
