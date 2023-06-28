using Sample.MySql.Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;

namespace Sample.MySql.Shardings
{
    public class RangeOrderTableRoute : AbstractShardingOperatorVirtualTableRoute<RangeOrder, long>
    {
        public override string ShardingKeyToTail(object shardingKey)
        {
            //取商
            var value = ((long)shardingKey) / (1000 * 10000);
            return value.ToString().PadLeft(4, '0'); //左补零 range_order_0000 range_order_0001 range_order_0002
        }

        public override List<string> GetTails()
        {
            //查询拨号器现在是多少位
            long nextId = 0;
            var id = nextId / (1000 * 10000);
            var tails = new List<string>();
            for (int i = 0; i <= id; i++)
            {
                var tail = i.ToString().PadLeft(4, '0');
                tails.Add(tail);
            }

            return tails;
        }

        public override void Configure(EntityMetadataTableBuilder<RangeOrder> builder)
        {
            builder.ShardingProperty(o => o.OrderId);
        }

        public override Func<string, bool> GetRouteToFilter(long shardingKey, ShardingOperatorEnum shardingOperator)
        {
            //当前值是对应表后缀多少
            var t = ShardingKeyToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) >= 0;
                case ShardingOperatorEnum.LessThan:
                {
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
                }
                case ShardingOperatorEnum.LessThanOrEqual:
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
                case ShardingOperatorEnum.Equal: return tail => tail == t;
                default:
                {
                    return tail => true;
                }
            }
        }
    }
}