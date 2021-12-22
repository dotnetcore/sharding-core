using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShardingCore.Core;
using ShardingCore.VirtualRoutes.Months;
using System;
using ShardingCore.Core.EntityMetadatas;

namespace Sample.Migrations.EFCores
{
    public class ShardingWithDateTime
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public class ShardingWithDateTimeMap : IEntityTypeConfiguration<ShardingWithDateTime>
    {
        public void Configure(EntityTypeBuilder<ShardingWithDateTime> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().IsUnicode(false).HasMaxLength(128);
            builder.Property(o => o.Name).HasMaxLength(128).HasComment("用户姓名");
            builder.ToTable(nameof(ShardingWithDateTime));
        }
    }
    public class ShardingWithDateTimeVirtualTableRoute : AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute<ShardingWithDateTime>
    {
        public override DateTime GetBeginTime()
        {
            return new DateTime(2021, 9, 1);
        }

        public override void Configure(EntityMetadataTableBuilder<ShardingWithDateTime> builder)
        {
            builder.ShardingProperty(o => o.CreateTime);
        }

        public override bool AutoCreateTableByTime()
        {
            return true;
        }
    }
}
