using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Test50.MySql.Domain.Entities;
using ShardingCore.VirtualRoutes;
using ShardingCore.VirtualRoutes.Mods;

namespace ShardingCore.Test50.MySql.Shardings
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 14 January 2021 15:39:27
* @Email: 326308290@qq.com
*/
    public class SysUserModVirtualTableRoute : AbstractSimpleShardingModKeyStringVirtualTableRoute<SysUserMod>
    {
        public SysUserModVirtualTableRoute() : base(2,3)
        {
        }

    }
}