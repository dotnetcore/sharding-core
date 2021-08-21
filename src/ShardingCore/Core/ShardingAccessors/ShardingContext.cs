using System;
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
                _shardingTables.Add(physicTable.EntityType, physicTable.Tail);
            }
        }

        /// <summary>
        /// 分表操作上下文 key:物理表名 value:虚拟表和本次分表tails
        /// </summary>
        private readonly Dictionary<Type, string> _shardingTables = new Dictionary<Type, string>();


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
        /// <param name="entityType"></param>
        /// <returns></returns>
        public string GetContextQueryTail(Type entityType)
        {
            if (_shardingTables.ContainsKey(entityType))
                return _shardingTables[entityType];
            return null;
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