using System.Collections.Generic;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Extensions;

namespace ShardingCore.Core.ShardingAccessors
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 22 December 2020 15:04:47
* @Email: 326308290@qq.com
*/
    public class ShardingContext
    {
        private ShardingContext(RouteResult routeResult)
        {
            foreach (var physicTable in routeResult.ReplaceTables)
            {
                _shardingTables.Add(physicTable.VirtualTable, new List<string>(1){physicTable.Tail});
            }
        }

        /// <summary>
        /// 分表操作上下文 key:物理表名 value:虚拟表和本次分表tails
        /// </summary>
        private readonly Dictionary<IVirtualTable, List<string>> _shardingTables = new Dictionary<IVirtualTable, List<string>>();

        /// <summary>
        /// 尝试添加本次操作表
        /// </summary>
        /// <param name="virtualTable"></param>
        /// <param name="tails"></param>
        /// <returns></returns>
        public void TryAddShardingTable(IVirtualTable virtualTable, List<string> tails)
        {
             _shardingTables.Add(virtualTable, tails);
        }

        /// <summary>
        /// 创建一个分表上下文
        /// </summary>
        /// <returns></returns>
        public static ShardingContext Create(RouteResult routeResult)
        {
            return new ShardingContext(routeResult);
        }

        /// <summary>
        /// 获取分表信息
        /// </summary>
        /// <param name="virtualTable"></param>
        /// <returns></returns>
        public List<string> GetContextQueryTails(IVirtualTable virtualTable)
        {
            if (_shardingTables.ContainsKey(virtualTable))
                return _shardingTables[virtualTable] ?? new List<string>(0);
            return new List<string>(0);
        }

        /// <summary>
        /// 是否是空的
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return _shardingTables.IsEmpty();
        }
    }
    
}