using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShardingCore.Core;
using ShardingCore.Core.QueryRouteManagers;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Exceptions;

namespace ShardingCore.Extensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/23 22:19:24
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public static class ShardingRouteExtension
    {
        /// <summary>
        /// 创建或者添加强制路由
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="shardingRouteContext"></param>
        /// <param name="tails"></param>
        /// <returns>任何一个tails被添加成功就返回成功</returns>
        public static bool TryCreateOrAddMustTail<TEntity>(this ShardingRouteContext shardingRouteContext, params string[] tails) where TEntity : class, IShardingTable
        {
            return TryCreateOrAddMustTail(shardingRouteContext, typeof(TEntity), tails);
        }
        /// <summary>
        /// 创建或者添加强制路由
        /// </summary>
        /// <param name="shardingRouteContext"></param>
        /// <param name="entityType"></param>
        /// <param name="tails"></param>
        /// <returns>任何一个tails被添加成功就返回成功</returns>
        public static bool TryCreateOrAddMustTail(this ShardingRouteContext shardingRouteContext, Type entityType, params string[] tails)
        {
            if (shardingRouteContext == null)
            {
                return false;
            }

            if (tails.IsEmpty())
                return false;
            if (!entityType.IsShardingTable())
                throw new ShardingCoreException($"sharding route entity type :{entityType.FullName} must impl {nameof(IShardingTable)}");
            if (!shardingRouteContext.Must.TryGetValue(entityType,out HashSet<string> mustTails))
            {
                mustTails = new HashSet<string>();
                shardingRouteContext.Must.Add(entityType, mustTails);
            }

            return tails.Select(o => mustTails.Add(o)).Any(o => o);
        }
        /// <summary>
        /// 创建或者添加提示路由
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="shardingRouteContext"></param>
        /// <param name="tails"></param>
        /// <returns>任何一个tails被添加成功就返回成功</returns>
        public static bool TryCreateOrAddHintTail<TEntity>(this ShardingRouteContext shardingRouteContext, params string[] tails) where TEntity : class, IShardingTable
        {
            return TryCreateOrAddHintTail(shardingRouteContext, typeof(TEntity), tails);
        }
        /// <summary>
        /// 创建或者添加提示路由
        /// </summary>
        /// <param name="shardingRouteContext"></param>
        /// <param name="entityType"></param>
        /// <param name="tails"></param>
        /// <returns>任何一个tails被添加成功就返回成功</returns>
        public static bool TryCreateOrAddHintTail(this ShardingRouteContext shardingRouteContext, Type entityType, params string[] tails)
        {
            if (shardingRouteContext == null)
            {
                return false;
            }

            if (tails.IsEmpty())
                return false;
            if (!entityType.IsShardingTable())
                throw new ShardingCoreException($"sharding route entity type :{entityType.FullName} must impl {nameof(IShardingTable)}");
            if (!shardingRouteContext.Hint.TryGetValue(entityType, out HashSet<string> hintTails))
            {
                hintTails = new HashSet<string>();
                shardingRouteContext.Hint.Add(entityType, hintTails);
            }

            return tails.Select(o => hintTails.Add(o)).Any(o => o);
        }
        /// <summary>
        /// 创建或者添加断言
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="shardingRouteContext"></param>
        /// <param name="tails"></param>
        /// <returns></returns>
        public static bool TryCreateOrAddAssertTail<TEntity>(this ShardingRouteContext shardingRouteContext, params IRouteAssert<TEntity>[] tails) where TEntity : class, IShardingTable
        {
            return TryCreateOrAddAssertTail(shardingRouteContext, typeof(TEntity), tails);
        }
        public static bool TryCreateOrAddAssertTail(this ShardingRouteContext shardingRouteContext, Type entityType, params IRouteAssert[] asserts)
        {
            if (shardingRouteContext == null)
            {
                return false;
            }

            if (asserts.IsEmpty())
                return false;
            if (!entityType.IsShardingTable())
                throw new ShardingCoreException($"sharding route entity type :{entityType.FullName} must impl {nameof(IShardingTable)}");
            if (!shardingRouteContext.Assert.TryGetValue(entityType, out LinkedList<IRouteAssert> routeAsserts))
            {
                routeAsserts = new LinkedList<IRouteAssert>();
                shardingRouteContext.Assert.Add(entityType, routeAsserts);
            }
            foreach (var routeAssert in asserts)
            {
                routeAsserts.AddLast(routeAssert);
            }

            return true;
        }



        public static bool TryGetMustTail<TEntity>(this ShardingRouteContext shardingRouteContext, out HashSet<string> tail) where TEntity  : class,IShardingTable
        {
            return TryGetMustTail(shardingRouteContext,typeof(TEntity),out tail);
        }
        public static bool TryGetMustTail(this ShardingRouteContext shardingRouteContext,Type entityType, out HashSet<string> tail)
        {
            if (shardingRouteContext == null)
            {
                tail = null;
                return false;
            }
            if (!entityType.IsShardingTable())
                throw new ShardingCoreException($"sharding route entity type :{entityType.FullName} must impl {nameof(IShardingTable)}");
            if (!shardingRouteContext.Must.ContainsKey(entityType))
            {
                tail = null;
                return false;
            }

            tail = shardingRouteContext.Must[entityType];
            return true;
        }
        public static bool TryGetHintTail<TEntity>(this ShardingRouteContext shardingRouteContext, out HashSet<string> tail) where TEntity  : class,IShardingTable
        {
            return TryGetHintTail(shardingRouteContext,typeof(TEntity),out tail);
        }
        public static bool TryGetHintTail(this ShardingRouteContext shardingRouteContext,Type entityType, out HashSet<string> tail)
        {
            if (shardingRouteContext == null)
            {
                tail = null;
                return false;
            }
            if (!entityType.IsShardingTable())
                throw new ShardingCoreException($"sharding route entity type :{entityType.FullName} must impl {nameof(IShardingTable)}");
            if (!shardingRouteContext.Hint.ContainsKey(entityType))
            {
                tail = null;
                return false;
            }

            tail = shardingRouteContext.Hint[entityType];
            return true;
        }

        public static bool TryGetAssertTail<TEntity>(this ShardingRouteContext shardingRouteContext, out ICollection<IRouteAssert> tail)where TEntity  : class,IShardingTable
        {
            return TryGetAssertTail(shardingRouteContext,typeof(TEntity), out tail);
        }
        public static bool TryGetAssertTail(this ShardingRouteContext shardingRouteContext,Type entityType, out ICollection<IRouteAssert> tail)
        {
            if (shardingRouteContext == null)
            {
                tail = null;
                return false;
            }
            if (!entityType.IsShardingTable())
                throw new ShardingCoreException($"sharding route entity type :{entityType.FullName} must impl {nameof(IShardingTable)}");
            if (!shardingRouteContext.Assert.ContainsKey(entityType))
            {
                tail = null;
                return false;
            }

            tail = shardingRouteContext.Assert[entityType];
            return true;
        }
        
    }
}
