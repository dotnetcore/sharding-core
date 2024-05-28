using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding
{
    /// <summary>
    /// 默认的链接字符串管理器
    /// </summary>
    public class DefaultConnectionStringManager : IConnectionStringManager
    {
        private readonly IVirtualDataSource _virtualDataSource;

        public DefaultConnectionStringManager(IVirtualDataSource virtualDataSource)
        {
            _virtualDataSource = virtualDataSource;
        }
        /// <summary>
        /// 获取链接字符串根据数据源名称
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        public string GetConnectionString(string dataSourceName)
        {
            if (_virtualDataSource.IsDefault(dataSourceName))
                return _virtualDataSource.DefaultConnectionString;
            return _virtualDataSource.GetPhysicDataSource(dataSourceName).ConnectionString;
        }
    }
}
