using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Sample.SqlServer.Domain.Entities;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.VirtualRoutes;

namespace Sample.SqlServer.Shardings
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 14 January 2021 15:39:27
* @Email: 326308290@qq.com
*/
    public class SysUserModVirtualRoute : AbstractSimpleShardingModVirtualRoute<SysUserMod, string>
    {
        public SysUserModVirtualRoute() : base(3)
        {
        }

        protected override string ConvertToShardingKey(object shardingKey)
        {
            return shardingKey.ToString();
        }

        public override List<string> GetAllTails()
        {
            return new() { "0","1","2"};
        }

        protected override Expression<Func<string, bool>> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = ShardingKeyToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.Equal: return tail => tail == t;
                default:
                {
                    Console.WriteLine($"shardingOperator is not equal scan all table tail");
                    return tail => true;
                }
            }
        }

    }
}