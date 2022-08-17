using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sample.SqlServer.Domain.Entities;

namespace Sample.SqlServer.Domain.Maps
{
    public class SysTestMap:IEntityTypeConfiguration<SysTest>
    {
        public void Configure(EntityTypeBuilder<SysTest> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).ValueGeneratedOnAdd().IsRequired().HasMaxLength(128);
            builder.Property(o => o.UserId).IsRequired().HasMaxLength(128);
            builder.ToTable(nameof(SysTest));
        }
    }
}