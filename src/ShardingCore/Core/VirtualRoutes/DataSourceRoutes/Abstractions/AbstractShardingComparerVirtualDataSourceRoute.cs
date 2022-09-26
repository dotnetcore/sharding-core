using System;
using System.Collections.Generic;
using ShardingCore.Core.EntityMetadatas;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions
{
    public abstract class AbstractShardingComparerVirtualDataSourceRouteclass<TEntity, TKey> :  AbstractShardingOperatorVirtualDataSourceRoute<TEntity, TKey> where TEntity : class
    {

        protected abstract IComparer<string> GetComparer();
        public override Func<string, bool> GetRouteToFilter(TKey shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var dataSourceName = ShardingKeyToDataSourceName(shardingKey);
            var comparer = GetComparer();
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    return dataSource => comparer.Compare(dataSource, dataSourceName) >= 0;
                case ShardingOperatorEnum.LessThan:
                case ShardingOperatorEnum.LessThanOrEqual:
                    return dataSource => comparer.Compare(dataSource, dataSourceName) <= 0;
                case ShardingOperatorEnum.Equal: return dataSource => comparer.Compare(dataSource, dataSourceName)==0;
                default:
                {
#if DEBUG
                    Console.WriteLine($"shardingOperator is not equal scan all table tail");
#endif
                    return dataSource => true;
                }
            }
        }
    } 
}
