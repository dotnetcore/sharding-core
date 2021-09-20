using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/7 10:32:26
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DefaultConnectionStringManager<TShardingDbContext> : IConnectionStringManager<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IVirtualDataSource<TShardingDbContext> _virtualDataSource;

        public DefaultConnectionStringManager(IVirtualDataSource<TShardingDbContext> virtualDataSource)
        {
            _virtualDataSource = virtualDataSource;
        }
        public string GetConnectionString(string dataSourceName)
        {
            return _virtualDataSource.GetPhysicDataSource(dataSourceName).ConnectionString;
        }
    }
}
