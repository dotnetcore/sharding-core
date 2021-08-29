using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samples.AutoByDate.SqlServer.Domain.Entities;

namespace Samples.AutoByDate.SqlServer.Domain.Maps
{
    public class TestLogByWeekMap:IEntityTypeConfiguration<TestLogByWeek>
    {
        public void Configure(EntityTypeBuilder<TestLogByWeek> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().HasMaxLength(128);
            builder.ToTable((nameof(TestLogByWeek)));
        }
    }
}
