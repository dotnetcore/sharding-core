using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/16 12:57:43
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 分库路由上下文
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataSourceRouteRuleContext
    {
        public DataSourceRouteRuleContext(IQueryable queryable,IShardingDbContext shardingDbContext)
        {
            Queryable = queryable;
            ShardingDbContext = shardingDbContext;
            VirtualDataSource = shardingDbContext.GetVirtualDataSource();
            QueryEntities = queryable.ParseQueryableEntities(shardingDbContext.GetType());
        }
        public ISet<Type> QueryEntities { get; }
        /// <summary>
        /// 查询条件
        /// </summary>
        public IQueryable Queryable { get; }

        public IShardingDbContext ShardingDbContext { get; }
        public IVirtualDataSource VirtualDataSource { get; }
    }
}
