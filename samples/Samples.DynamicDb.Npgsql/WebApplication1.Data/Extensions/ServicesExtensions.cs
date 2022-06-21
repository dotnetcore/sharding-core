using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication1.Data;
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
            var dblist = JsonFileHelper.Read<List<string>>(AppContext.BaseDirectory, TestModelVirtualDataSourceRoute.ConfigFileName) ?? new List<string>();
            foreach (var key in dblist)
            {
                DynamicShardingHelper.DynamicAppendDataSource<AbstaractShardingDbContext>("c1", key, $"server=127.0.0.1;port=5432;uid=postgres;pwd=3#SanJing;database=shardingCoreDemo_{key};");

            }
            return serviceProvider;
            //using (var scope = serviceProvider.CreateScope())
            //{
            //    var db = scope.ServiceProvider.GetRequiredService<AbstaractShardingDbContext>();
            //    db.Database.EnsureCreated();

            //    var dbKeys = db.TestModelKeys.ToList();
            //    if (dbKeys.Any())
            //    {
            //        foreach (var item in dbKeys)
            //        {
            //            DynamicShardingHelper.DynamicAppendDataSource<AbstaractShardingDbContext>("c1", item.Key, $"server=127.0.0.1;port=5432;uid=postgres;pwd=3#SanJing;database=shardingCoreDemo_{item.Key};");
            //        }
            //    }
            //}
        }

    }
}
