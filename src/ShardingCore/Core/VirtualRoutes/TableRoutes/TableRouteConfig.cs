using System.Linq;
using System.Linq.Expressions;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 14:15:02
* @Email: 326308290@qq.com
*/
    public class TableRouteConfig
    {
        private readonly IQueryable _queryable;
        private readonly IShardingEntity _shardingEntity;
        private readonly object _shardingKeyValue;
        private readonly Expression _predicate;


        public TableRouteConfig(IQueryable queryable=null,IShardingEntity shardingEntity=null,object shardingKeyValue=null,Expression predicate=null)
        {
            _queryable = queryable;
            _shardingEntity = shardingEntity;
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

        public IShardingEntity GetShardingEntity()
        {
            return _shardingEntity;
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
            return _shardingEntity != null;
        }

        public bool UsePredicate()
        {
            return _predicate != null;
        }
    }
}