using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;
using ShardingCore.VirtualRoutes.Months;

namespace Sample.Migrations.EFCores
{
    public class ShardingWithMod
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string TextStr { get; set; }
        public string TextStr1 { get; set; }
        public string TextStr2 { get; set; }
    }

    public class ShardingWithModMap : IEntityTypeConfiguration<ShardingWithMod>
    {
        public void Configure(EntityTypeBuilder<ShardingWithMod> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().IsUnicode(false).HasMaxLength(128);
            builder.Property(o => o.Name).HasMaxLength(128).HasComment("用户姓名");
            builder.Property(o => o.TextStr).IsRequired().HasMaxLength(128).HasDefaultValue("").HasComment("值123");
            builder.Property(o => o.TextStr1).IsRequired().HasMaxLength(128).HasDefaultValue("123");
            builder.Property(o => o.TextStr2).IsRequired().HasMaxLength(128).HasDefaultValue("123");
            builder.ToTable(nameof(ShardingWithMod));
        }
    }
    public class ShardingWithModVirtualTableRoute : AbstractSimpleShardingModKeyStringVirtualTableRoute<ShardingWithMod>
    {
        public ShardingWithModVirtualTableRoute() : base(2, 3)
        {
        }

        public override void Configure(EntityMetadataTableBuilder<ShardingWithMod> builder)
        {
            builder.ShardingProperty(o => o.Id);
        }
    }
}
