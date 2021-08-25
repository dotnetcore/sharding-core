using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Sample.SqlServer.Domain.Entities;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;

namespace Sample.SqlServer.Shardings
{
    public class TestVRoute : AbstractShardingOperatorVirtualTableRoute<SysUserMod, string> 
    {
        protected override string ConvertToShardingKey(object shardingKey)
        {
            return shardingKey.ToString();
        }

        public override string ShardingKeyToTail(object shardingKey)
        {
            throw new NotImplementedException();
        }

        //数据库已经存在的tail
        public override List<string> GetAllTails()
        {
            return new List<string>() {"", "1"};
        }

        protected override Expression<Func<string, bool>> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
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
