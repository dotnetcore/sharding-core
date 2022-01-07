using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sample.MultiConfig.Domain.Entities;

namespace Sample.MultiConfig.Domain.Maps
{
    public class OrderMap:IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsUnicode(false).HasMaxLength(50);
            builder.Property(o => o.Name).HasMaxLength(100);
            builder.ToTable(nameof(Order));
        }
    }
}
