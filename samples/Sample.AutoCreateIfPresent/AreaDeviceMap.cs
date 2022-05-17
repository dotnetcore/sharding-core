using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sample.AutoCreateIfPresent
{
    public class AreaDeviceMap:IEntityTypeConfiguration<AreaDevice>
    {
        public void Configure(EntityTypeBuilder<AreaDevice> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().HasMaxLength(50);
            builder.Property(o => o.Area).IsRequired().HasMaxLength(128);
            builder.ToTable(nameof(AreaDevice));
        }
    }
}
