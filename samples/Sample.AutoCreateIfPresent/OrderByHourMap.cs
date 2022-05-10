using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/*
* @Author: xjm
* @Description:
* @Date: DATE
* @Email: 326308290@qq.com
*/
namespace Sample.AutoCreateIfPresent
{
    public class OrderByHourMap:IEntityTypeConfiguration<OrderByHour>
    {
        public void Configure(EntityTypeBuilder<OrderByHour> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().HasMaxLength(50);
            builder.Property(o => o.Name).IsRequired().HasMaxLength(128);
            builder.ToTable(nameof(OrderByHour));
        }
    }
}