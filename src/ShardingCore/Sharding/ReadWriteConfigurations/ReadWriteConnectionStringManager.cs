using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/7 10:37:28
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ReadWriteConnectionStringManager<TShardingDbContext> : IConnectionStringManager<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private IShardingConnectionStringResolver<TShardingDbContext> _shardingConnectionStringResolver;
        private readonly IVirtualDataSource<TShardingDbContext> _virtualDataSource;


        public ReadWriteConnectionStringManager(IShardingConnectionStringResolver<TShardingDbContext> shardingConnectionStringResolver,IVirtualDataSource<TShardingDbContext> virtualDataSource)
        {
            _shardingConnectionStringResolver = shardingConnectionStringResolver;
            _virtualDataSource = virtualDataSource;
        }
        public string GetConnectionString(string dataSourceName)
        {
            if (!_shardingConnectionStringResolver.ContainsReadWriteDataSourceName(dataSourceName))
                return _virtualDataSource.GetConnectionString(dataSourceName);
            return _shardingConnectionStringResolver.GetConnectionString(dataSourceName);
           
        }
    }
}
