using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.DbContexts.Abstractions;
using ShardingCore.DbContexts.ShardingDbContexts;

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
        void AddShardingDbContextWithShardingTable<T>(string connectKey, string connectString,
            Action<ShardingDbConfigOptions> func) where T : DbContext, IShardingTableDbContext;

         void AddShardingDbContext<T>(string connectKey, string connectString) where T : DbContext;


         void AddDataSourceVirtualRoute<TRoute>() where TRoute : IDataSourceVirtualRoute;


         ISet<ShardingConfigEntry> GetShardingConfigs();
         ShardingConfigEntry GetShardingConfig(string connectKey);
         ISet<Type> GetVirtualRoutes();
         Type GetVirtualRoute(Type entityType);



         /// <summary>
         /// 如果数据库不存在就创建并且创建表除了分表的
         /// </summary>
         bool EnsureCreatedWithOutShardingTable { get; set; }
         /// <summary>
         /// 是否需要在启动时创建分表
         /// </summary>
         bool? CreateShardingTableOnStart { get; set; }

         /// <summary>
         /// 添加filter过滤器
         /// </summary>
         /// <typeparam name="TFilter"></typeparam>
         public void AddDbContextCreateFilter<TFilter>() where TFilter : class, IDbContextCreateFilter;

         public List<Type> GetFilters();

    }
}
