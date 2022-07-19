using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sample.MySql.Domain.Entities;

namespace Sample.MySql.Domain.Maps
{
    public class SysUserLogByMonthMap : IEntityTypeConfiguration<SysUserLogByMonth>
    {
        public void Configure(EntityTypeBuilder<SysUserLogByMonth> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().HasMaxLength(128);
            builder.ToTable("Sys_User_LogBy_Month");
        }
    }
}