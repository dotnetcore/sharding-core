using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Extensions;

namespace ShardingCore.MySql
{
/*
* @Author: xjm
* @Description:
* @Date: 2020年4月7日 8:34:04
* @Email: 326308290@qq.com
*/
    public class MySqlOptions
    {
        public LinkedList<Type> ShardingRoutes=new LinkedList<Type>();
        public  string ConnectionString { get; set; }
#if EFCORE5
        public MySqlServerVersion ServerVersion { get; set; }
#endif
        
        public Action<MySqlDbContextOptionsBuilder> MySqlOptionsAction  { get; set; }

        public void AddSharding<TRoute>()where TRoute:IVirtualRoute
        {
            ShardingRoutes.AddLast(typeof(TRoute));
        }

        public bool HasSharding => ShardingRoutes.IsNotEmpty();
        public Action<IServiceProvider, ShardingCoreConfig> ShardingCoreConfigConfigure { get; private set; }

        public void SetMySqlOptions(Action<MySqlDbContextOptionsBuilder> action)
        {
            MySqlOptionsAction = action;
        }
        public void CreateIfNotExists(Action<IServiceProvider, ShardingCoreConfig> action)
        {
            ShardingCoreConfigConfigure = action;
        }
    }
}