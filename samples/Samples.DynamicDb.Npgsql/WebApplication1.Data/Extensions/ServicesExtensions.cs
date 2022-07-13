using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Helpers;
using System;
using System.Linq;
using WebApplication1.Data.Helpers;
using WebApplication1.Data.Sharding;

namespace WebApplication1.Data.Extensions
{
    public static class ServicesExtensions
    {

        /// <summary>
        /// 初始化动态数据库
        /// </summary>
        /// <param name="serviceProvider"></param>
        public static IServiceProvider InitialDynamicVirtualDataSource(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AbstaractShardingDbContext>();
                var runtimeContext = scope.ServiceProvider.GetRequiredService<IShardingRuntimeContext>();
                //db.Database.EnsureCreated();

                var dblist = db.TestModelKeys.Select(m => m.Key).ToList();
                // 存入到动态库配置文件缓存中
                JsonFileHelper.Save(AppContext.BaseDirectory, TestModelVirtualDataSourceRoute.ConfigFileName, dblist);

                // 遍历添加动态数据源
                foreach (var key in dblist)
                {
                    DynamicShardingHelper.DynamicAppendDataSourceOnly(runtimeContext, key, $"server=127.0.0.1;port=5432;uid=postgres;pwd=3#SanJing;database=shardingCoreDemo_{key};");
                }

                db.Database.Migrate();
            }

            return serviceProvider;
        }

    }
}
