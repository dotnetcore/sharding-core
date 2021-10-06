using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sample.Migrations.EFCores
{
    public class NoShardingTable
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class NoShardingTableMap : IEntityTypeConfiguration<NoShardingTable>
    {
        public void Configure(EntityTypeBuilder<NoShardingTable> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().IsUnicode(false).HasMaxLength(128);
            builder.Property(o => o.Name).HasMaxLength(128);
            builder.ToTable(nameof(NoShardingTable));
        }
    }
}
