using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/4 13:11:16
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IShardingCoreOptions
    {
         void AddShardingDbContext<T>(string connectKey, string connectString, Action<ShardingDbConfigOptions> func) where T : DbContext;

         void AddShardingDbContext<T>(string connectKey, string connectString) where T : DbContext;


         void AddDataSourceVirtualRoute<TRoute>() where TRoute : IDataSourceVirtualRoute;


         ISet<ShardingConfigEntry> GetShardingConfigs();
         ShardingConfigEntry GetShardingConfig(string connectKey);
         ISet<Type> GetVirtualRoutes();
         Type GetVirtualRoute(Type entityType);

    }
}
