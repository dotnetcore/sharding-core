// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using ShardingCore.Test50.MySql.Domain.Entities;
//
// namespace ShardingCore.Test50.MySql.Domain.Maps
// {
// /*
// * @Author: xjm
// * @Description:
// * @Date: Wednesday, 20 January 2021 10:45:47
// * @Email: 326308290@qq.com
// */
//     public class SysUserRangeMap:IEntityTypeConfiguration<SysUserRange>
//     {
//         public void Configure(EntityTypeBuilder<SysUserRange> builder)
//         {
//             builder.HasKey(o => o.Id);
//             builder.Property(o => o.Id).IsRequired().HasMaxLength(128);
//             builder.Property(o => o.Name).HasMaxLength(128);
//             builder.ToTable(nameof(SysUserRange));
//         }
//     }
// }