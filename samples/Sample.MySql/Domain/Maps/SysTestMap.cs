using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sample.MySql.Domain.Entities;

namespace Sample.MySql.Domain.Maps
{
    public class SysTestMap:IEntityTypeConfiguration<SysTest>
    {
        public void Configure(EntityTypeBuilder<SysTest> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().HasMaxLength(128);
            builder.Property(o => o.UserId).IsRequired().HasMaxLength(128);
            builder.Property(o => o.UserId).IsConcurrencyToken();
            builder.HasIndex(o => new { o.Id, o.UserId });
            builder.ToTable(nameof(SysTest),"dbdbd0");
        }
    }
}