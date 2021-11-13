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


        public ReadWriteConnectionStringManager(IShardingConnectionStringResolver<TShardingDbContext> shardingConnectionStringResolver)
        {
            _shardingConnectionStringResolver = shardingConnectionStringResolver;
        }
        public string GetConnectionString(string dataSourceName)
        {
            return _shardingConnectionStringResolver.GetConnectionString(dataSourceName);
           
        }
    }
}
