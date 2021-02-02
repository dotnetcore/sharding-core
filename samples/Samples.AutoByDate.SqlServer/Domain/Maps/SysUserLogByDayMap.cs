using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samples.AutoByDate.SqlServer.Domain.Entities;

namespace Samples.AutoByDate.SqlServer.Domain.Maps
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 02 February 2021 17:10:55
* @Email: 326308290@qq.com
*/
    public class SysUserLogByDayMap:IEntityTypeConfiguration<SysUserLogByDay>
    {
        public void Configure(EntityTypeBuilder<SysUserLogByDay> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).ValueGeneratedOnAdd();
            builder.Property(o => o.Body).HasMaxLength(128);
            builder.ToTable(nameof(SysUserLogByDay));
        }
    }
}