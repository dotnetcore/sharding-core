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
    public class DefaultConnectionStringManager : IConnectionStringManager
    {
        private readonly IVirtualDataSource _virtualDataSource;

        public DefaultConnectionStringManager(IVirtualDataSource virtualDataSource)
        {
            _virtualDataSource = virtualDataSource;
        }
        public string GetConnectionString(string dataSourceName)
        {
            if (_virtualDataSource.IsDefault(dataSourceName))
                return _virtualDataSource.DefaultConnectionString;
            return _virtualDataSource.GetPhysicDataSource(dataSourceName).ConnectionString;
        }
    }
}
