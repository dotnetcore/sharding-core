using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Sample.SqlServerShardingTable.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Helpers;

namespace Sample.SqlServerShardingTable.VirtualRoutes
{
    public class OrderHashRangeVirtualTableRoute:AbstractShardingOperatorVirtualTableRoute<Order,string>
    {

        public override string ShardingKeyToTail(object shardingKey)
        {
            var stringHashCode = ShardingCoreHelper.GetStringHashCode(shardingKey.ToString());
            var hashCode = stringHashCode % 10000;
            if (hashCode >= 0 && hashCode <= 3000)
            {
                return "A";
            }
            else if (hashCode >= 3001 && hashCode <= 6000)
            {
                return "B";
            }
            else if (hashCode >= 6001 && hashCode <= 10000)
            {
                return "C";
            }
            else
                throw new ShardingCoreInvalidOperationException($"cant calc hash route hash code:[{stringHashCode}]");
        }

        public override List<string> GetTails()
        {
            return new List<string>()
            {
                "A", "B", "C"
            };
        }

        public override void Configure(EntityMetadataTableBuilder<Order> builder)
        {
            
        }

        public override Func<string, bool> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
        {
            //因为hash路由仅支持等于所以仅仅只需要写等于的情况
            var t = ShardingKeyToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.Equal: return tail => tail == t;
                default:
                {
                    return tail => true;
                }
            }
        }
    }
}
