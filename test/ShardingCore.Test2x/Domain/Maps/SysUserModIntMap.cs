using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShardingCore.Test2x.Domain.Entities;

namespace ShardingCore.Test2x.Domain.Maps
{
    public class SysUserModIntMap:IEntityTypeConfiguration<SysUserModInt>
    {
        public void Configure(EntityTypeBuilder<SysUserModInt> builder)
        {

            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).ValueGeneratedNever();
            builder.Property(o => o.Name).HasMaxLength(128);
            builder.ToTable(nameof(SysUserModInt));
        }
    }
}
