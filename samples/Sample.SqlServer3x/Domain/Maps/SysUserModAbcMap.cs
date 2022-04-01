using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sample.SqlServer3x.Domain.Entities;

namespace Sample.SqlServer3x.Domain.Maps
{
    public class SysUserModAbcMap:IEntityTypeConfiguration<SysUserModAbc>
    {
        public void Configure(EntityTypeBuilder<SysUserModAbc> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().HasMaxLength(128);
            builder.Property(o => o.Name).HasMaxLength(128);
            builder.ToTable(nameof(SysUserModAbc));
        }
    }
}
