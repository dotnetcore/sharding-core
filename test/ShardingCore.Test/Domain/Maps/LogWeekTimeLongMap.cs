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
    public class LogWeekTimeLongMap : IEntityTypeConfiguration<LogWeekTimeLong>
    {
        public void Configure(EntityTypeBuilder<LogWeekTimeLong> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().IsUnicode(false).HasMaxLength(50);
            builder.Property(o => o.Body).HasMaxLength(128);
            builder.ToTable(nameof(LogWeekTimeLong));
        }
    }
}
