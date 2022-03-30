using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.ShardingDbContextExecutors;

namespace ShardingCore.Sharding.Abstractions
{
    public interface IShardingDbContextDiscover
    {
        /// <summary>
        /// 获取当前拥有的所有db context
        /// </summary>
        /// <returns></returns>
        IDictionary<string, IDataSourceDbContext> GetDataSourceDbContexts();
    }
}
