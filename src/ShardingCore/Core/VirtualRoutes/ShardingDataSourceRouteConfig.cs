using System.Linq;
using System.Linq.Expressions;

namespace ShardingCore.Core.VirtualRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 06 February 2021 11:28:33
* @Email: 326308290@qq.com
*/
    public class ShardingDataSourceRouteConfig
    {
        
        private readonly IQueryable _queryable;
        private readonly object _shardingDataSource;
        private readonly object _shardingKeyValue;
        private readonly Expression _predicate;


        public ShardingDataSourceRouteConfig(IQueryable queryable=null,object shardingDataSource=null,object shardingKeyValue=null,Expression predicate=null)
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

        public object GetShardingDataSource()
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