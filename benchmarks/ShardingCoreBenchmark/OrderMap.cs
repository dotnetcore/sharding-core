using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShardingCore6x
{
    internal class OrderMap:IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).ValueGeneratedOnAdd().IsUnicode(false).HasMaxLength(50);
            builder.Property(o => o.Body).IsRequired().HasDefaultValue(string.Empty).HasMaxLength(128);
            builder.Property(o => o.Remark).IsRequired().HasDefaultValue(string.Empty).HasMaxLength(128);
            builder.Property(o => o.Payer).IsRequired().IsUnicode(false).HasMaxLength(50);
            builder.Property(o => o.OrderStatus).HasConversion<int>();
            builder.ToTable(nameof(Order));
        }
    }
}
