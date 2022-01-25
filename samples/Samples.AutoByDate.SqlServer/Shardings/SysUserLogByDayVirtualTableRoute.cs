using System;
using System.Collections.Generic;
using Samples.AutoByDate.SqlServer.Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Days;

namespace Samples.AutoByDate.SqlServer.Shardings
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 02 February 2021 17:14:53
* @Email: 326308290@qq.com
*/
    public class SysUserLogByDayVirtualTableRoute:AbstractSimpleShardingDayKeyDateTimeVirtualTableRoute<SysUserLogByDay>
    {
        public override DateTime GetBeginTime()
        {
            //必须返回固定值比如new DateTime(2021,1,1)
            //如果返回动态值会导致程序重新启动这个值就会变动导致无法获取之前的表
            return DateTime.Now.AddDays(-2);
        }

        public override bool AutoCreateTableByTime()
        {
            return true;
        }

        public override void Configure(EntityMetadataTableBuilder<SysUserLogByDay> builder)
        {
            builder.ShardingProperty(o => o.CreateTime);
        }
    }
}