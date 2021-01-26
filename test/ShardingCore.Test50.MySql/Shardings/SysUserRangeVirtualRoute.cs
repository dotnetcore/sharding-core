using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Helpers;
using ShardingCore.Test50.MySql.Domain.Entities;

namespace ShardingCore.Test50.MySql.Shardings
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 20 January 2021 10:46:37
* @Email: 326308290@qq.com
*/
    public class SysUserRangeVirtualRoute: AbstractShardingOperatorVirtualRoute<SysUserRange, string>
    {
        private int _mod = 1000;
        protected override string ConvertToShardingKey(object shardingKey)
        {
            return shardingKey.ToString();
        }

        public override string ShardingKeyToTail(object shardingKey)
        {
            var shardingKeyStr = ConvertToShardingKey(shardingKey);
            var m = Math.Abs(ShardingCoreHelper.GetStringHashCode(shardingKeyStr) % _mod);
            if (m > 800)//801-999
            {
                return "3";
            } else if (m > 600)//601-800
            {
                return "2";
            } else if (m > 300)//301-600
            {
                return "1";
            } else //0-300
            {
                return "0";
            }
        }

        public override List<string> GetAllTails()
        {
            return new(){"0", "1","2","3"};
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

        // public override List<IPhysicTable> AfterPhysicTableFilter(List<IPhysicTable> allPhysicTables, List<IPhysicTable> filterPhysicTables)
        // {
        //     if (filterPhysicTables.Count > 1)
        //         throw new Exception($"query {nameof(SysUserRange)} not support cross table");
        //     return base.AfterPhysicTableFilter(allPhysicTables, filterPhysicTables);
        // }
    }
}