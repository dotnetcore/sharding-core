using System;
using System.Collections.Generic;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Extensions;

namespace ShardingCore.SqlServer
{
/*
* @Author: xjm
* @Description:
* @Date: 2020年4月7日 8:34:04
* @Email: 326308290@qq.com
*/
    public class SqlServerOptions
    {
        public LinkedList<Type> ShardingRoutes=new LinkedList<Type>();
        public  string ConnectionString { get; set; }

        public void AddSharding<TRoute>()where TRoute:IVirtualRoute
        {
            ShardingRoutes.AddLast(typeof(TRoute));
        }

        public bool HasSharding => ShardingRoutes.IsNotEmpty();
        public Action<IServiceProvider, ShardingCoreConfig> ShardingCoreConfigConfigure { get; private set; }
        public void UseShardingCoreConfig(Action<IServiceProvider, ShardingCoreConfig> function)
        {
            ShardingCoreConfigConfigure = function;
        }
    }
}