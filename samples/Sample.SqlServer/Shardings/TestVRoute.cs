using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Sample.SqlServer.Domain.Entities;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;

namespace Sample.SqlServer.Shardings
{
    public class TestVRoute : AbstractShardingOperatorVirtualTableRoute<SysUserMod, string> 
    {
        public override string ShardingKeyToTail(object shardingKey)
        {
            return shardingKey.ToString();
        }

        //数据库已经存在的tail
        public override List<string> GetTails()
        {
            return new List<string>() {"", "1"};
        }

        public override void Configure(EntityMetadataTableBuilder<SysUserMod> builder)
        {
            builder.ShardingProperty(o => o.Id);
        }

        public override Func<string, bool> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = ShardingKeyToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.Equal: return tail => tail == t;
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
