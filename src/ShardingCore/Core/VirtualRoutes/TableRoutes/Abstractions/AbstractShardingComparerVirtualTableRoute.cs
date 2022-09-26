using System;
using System.Collections;
using System.Collections.Generic;
using ShardingCore.Core.EntityMetadatas;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions
{

    public abstract  class AbstractShardingComparerVirtualTableRoute<TEntity, TKey> : AbstractShardingOperatorVirtualTableRoute<TEntity, TKey> where TEntity : class
    {
        protected abstract IComparer<string> GetComparer();

        public override Func<string, bool> GetRouteToFilter(TKey shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = ShardingKeyToTail(shardingKey);
            var comparer = GetComparer();
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    return tail => comparer.Compare(tail, t) >= 0;
                case ShardingOperatorEnum.LessThan:
                case ShardingOperatorEnum.LessThanOrEqual:
                    return tail => comparer.Compare(tail, t) <= 0;
                case ShardingOperatorEnum.Equal: return tail => comparer.Compare(tail, t)==0;
                default:
                {
#if DEBUG
                    Console.WriteLine($"shardingOperator is not equal scan all table tail");
#endif
                    return tail => true;
                }
            }
        }
    }
}