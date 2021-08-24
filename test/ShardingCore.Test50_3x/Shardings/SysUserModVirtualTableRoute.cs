using ShardingCore.Test50_3x.Domain.Entities;
using ShardingCore.VirtualRoutes.Mods;

namespace ShardingCore.Test50_3x.Shardings
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 14 January 2021 15:39:27
* @Email: 326308290@qq.com
*/
    public class SysUserModVirtualTableRoute : AbstractSimpleShardingModKeyStringVirtualTableRoute<SysUserMod>
    {
        protected override bool EnableHintRoute => true;
        public SysUserModVirtualTableRoute() : base(2,3)
        {
        }

    }
}