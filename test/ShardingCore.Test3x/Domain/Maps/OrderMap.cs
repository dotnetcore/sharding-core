﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShardingCore.Test3x.Domain.Entities;

namespace ShardingCore.Test3x.Domain.Maps
{
    public class OrderMap:IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Area).IsRequired().IsUnicode(false).HasMaxLength(20);
            builder.ToTable(nameof(Order));
        }
    }
}
