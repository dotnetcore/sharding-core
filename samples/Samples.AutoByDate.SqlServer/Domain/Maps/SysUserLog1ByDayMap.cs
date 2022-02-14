using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samples.AutoByDate.SqlServer.Domain.Entities;

namespace Samples.AutoByDate.SqlServer.Domain.Maps
{
    public class SysUserLog1ByDayMap:IEntityTypeConfiguration<SysUserLog1ByDay>
    {
        public void Configure(EntityTypeBuilder<SysUserLog1ByDay> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).ValueGeneratedOnAdd();
            builder.Property(o => o.Body).HasMaxLength(128);
            //builder.HasOne(o => o.SysUserLog).WithOne(o => o.SysUserLog1)
            //    .HasForeignKey<SysUserLog1ByDay>(o => o.UserId);
            builder.ToTable(nameof(SysUserLog1ByDay));
        }
    }
}
