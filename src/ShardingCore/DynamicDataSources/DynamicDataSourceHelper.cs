using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using System;
using System.Collections.Generic;

namespace ShardingCore.DynamicDataSources
{
    public class DynamicDataSourceHelper
    {
        private DynamicDataSourceHelper()
        {
            throw new InvalidOperationException($"{nameof(DynamicDataSourceHelper)} create instance");
        }

        //public static void DynamicAppendDataSource<TShardingDbContext>(string dataSourceName, string connectionString) where TShardingDbContext:DbContext,IShardingDbContext
        //{
        //    var defaultDataSourceInitializer = ShardingContainer.GetService<IDataSourceInitializer<TShardingDbContext>>();
        //    defaultDataSourceInitializer.InitConfigure(dataSourceName, connectionString,false);
        //}

    }
}
