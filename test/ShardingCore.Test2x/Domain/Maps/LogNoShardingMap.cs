using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShardingCore.Test2x.Domain.Entities;

namespace ShardingCore.Test2x.Domain.Maps
{
    public class LogNoShardingMap:IEntityTypeConfiguration<LogNoSharding>
    {
        public void Configure(EntityTypeBuilder<LogNoSharding> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().HasMaxLength(50);
            builder.Property(o => o.Body).HasMaxLength(256);
            builder.ToTable(nameof(LogNoSharding));
        }
    }
}
