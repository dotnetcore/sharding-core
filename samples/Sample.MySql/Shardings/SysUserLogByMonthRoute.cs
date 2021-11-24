using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sample.MySql.Domain.Entities;
using ShardingCore.VirtualRoutes.Months;

namespace Sample.MySql.Shardings
{
    public class SysUserLogByMonthRoute:AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute<SysUserLogByMonth>
    {
        public override DateTime GetBeginTime()
        {
            return new DateTime(2021, 1, 01);
        }

        public override bool AutoCreateTableByTime()
        {
            return true;
        }
    }
}
