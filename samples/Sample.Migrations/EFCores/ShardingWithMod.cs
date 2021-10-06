using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShardingCore.Core;
using ShardingCore.VirtualRoutes.Mods;
using ShardingCore.VirtualRoutes.Months;

namespace Sample.Migrations.EFCores
{
    public class ShardingWithMod:IShardingTable
    {
        [ShardingTableKey]
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class ShardingWithModMap : IEntityTypeConfiguration<ShardingWithMod>
    {
        public void Configure(EntityTypeBuilder<ShardingWithMod> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().IsUnicode(false).HasMaxLength(128);
            builder.Property(o => o.Name).HasMaxLength(128);
            builder.ToTable(nameof(ShardingWithMod));
        }
    }
    public class ShardingWithModVirtualTableRoute : AbstractSimpleShardingModKeyStringVirtualTableRoute<ShardingWithMod>
    {
        public ShardingWithModVirtualTableRoute() : base(2, 3)
        {
        }
    }
}
