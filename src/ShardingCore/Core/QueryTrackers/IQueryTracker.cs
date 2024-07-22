using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.QueryTrackers
{
    public interface IQueryTracker
    {
        /// <summary>
        /// 追踪数据对象
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="shardingDbContext"></param>
        /// <returns></returns>
        public object Track(object entity,IShardingDbContext shardingDbContext);
    }
}
