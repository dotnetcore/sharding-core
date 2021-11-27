using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShardingCore.Test2x.Domain.Entities;

namespace ShardingCore.Test2x.Domain.Maps
{
    public class LogMonthLongMap:IEntityTypeConfiguration<LogMonthLong>
    {
        public void Configure(EntityTypeBuilder<LogMonthLong> builder)
        {

            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().IsUnicode(false).HasMaxLength(50);
            builder.Property(o => o.Body).HasMaxLength(128);
            builder.ToTable(nameof(LogMonthLong));
        }
    }
}
