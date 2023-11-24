using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShardingCore.Test.Domain.Entities;

namespace ShardingCore.Test.Domain.Maps
{
   public  class LogDayLongMap:IEntityTypeConfiguration<LogDayLong>
    {
        public void Configure(EntityTypeBuilder<LogDayLong> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.LogLevel).IsRequired().IsUnicode(false).HasMaxLength(32);
            builder.Property(o => o.LogBody).IsRequired().HasMaxLength(256);
            builder.ToTable(nameof(LogDayLong));
        }
    }
}
