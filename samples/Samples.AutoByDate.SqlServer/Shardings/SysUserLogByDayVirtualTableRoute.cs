using System;
using System.Collections.Generic;
using Samples.AutoByDate.SqlServer.Domain.Entities;
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
            return DateTime.Now.AddDays(-2);
        }
    }
}