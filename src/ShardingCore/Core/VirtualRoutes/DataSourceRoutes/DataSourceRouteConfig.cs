using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/2 16:08:34
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DataSourceRouteConfig
    {
        private readonly IQueryable _queryable;
        private readonly IShardingDataSource _shardingDataSource;
        private readonly object _shardingKeyValue;
        private readonly Expression _predicate;


        public DataSourceRouteConfig(IQueryable queryable = null, IShardingDataSource shardingDataSource = null, object shardingKeyValue = null, Expression predicate = null)
        {
            _queryable = queryable;
            _shardingDataSource = shardingDataSource;
            _shardingKeyValue = shardingKeyValue;
            _predicate = predicate;
        }

        public IQueryable GetQueryable()
        {
            return _queryable;
        }
        public object GetShardingKeyValue()
        {
            return _shardingKeyValue;
        }

        public IShardingDataSource GetShardingDataSource()
        {
            return _shardingDataSource;
        }

        public Expression GetPredicate()
        {
            return _predicate;
        }

        public bool UseQueryable()
        {
            return _queryable != null;
        }

        public bool UseValue()
        {
            return _shardingKeyValue != null;
        }

        public bool UseEntity()
        {
            return _shardingDataSource != null;
        }

        public bool UsePredicate()
        {
            return _predicate != null;
        }
    }
}
