using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShardingCore.Test.Domain.Entities;

namespace ShardingCore.Test.Domain.Maps
{
    public class LogDayMap:IEntityTypeConfiguration<LogDay>
    {
        public void Configure(EntityTypeBuilder<LogDay> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.LogLevel).IsRequired().IsUnicode(false).HasMaxLength(32);
            builder.Property(o => o.LogBody).IsRequired().HasMaxLength(256);
            builder.ToTable(nameof(LogDay));
        }
    }
}
