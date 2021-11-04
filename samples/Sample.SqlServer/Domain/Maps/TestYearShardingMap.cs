using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sample.SqlServer.Domain.Entities;

namespace Sample.SqlServer.Domain.Maps
{
    public class TestYearShardingMap:IEntityTypeConfiguration<TestYearSharding>
    {
        public void Configure(EntityTypeBuilder<TestYearSharding> builder)
        {
            builder.HasKey(o => o.Id);
            builder.ToTable(nameof(TestYearSharding));
        }
    }
}
