using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.DynamicDataSources
{
    public interface IDataSourceInitializer<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        /// <param name="isOnStart">当前是否是启动时被调用</param>
        void InitConfigure(string dataSourceName, string connectionString,bool isOnStart);
    }
}
