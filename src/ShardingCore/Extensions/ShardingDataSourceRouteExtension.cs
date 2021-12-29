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
    public static class ShardingDataSourceRouteExtension
    {
        /// <summary>
        /// 创建或者添加强制路由
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="shardingRouteContext"></param>
        /// <param name="dataSources"></param>
        /// <returns>任何一个dataSources被添加成功就返回成功</returns>
        public static bool TryCreateOrAddMustDataSource<TEntity>(this ShardingRouteContext shardingRouteContext, params string[] dataSources) where TEntity : class
        {
            return TryCreateOrAddMustDataSource(shardingRouteContext, typeof(TEntity), dataSources);
        }
        /// <summary>
        /// 创建或者添加强制路由
        /// </summary>
        /// <param name="shardingRouteContext"></param>
        /// <param name="entityType"></param>
        /// <param name="dataSources"></param>
        /// <returns>任何一个dataSources被添加成功就返回成功</returns>
        public static bool TryCreateOrAddMustDataSource(this ShardingRouteContext shardingRouteContext, Type entityType, params string[] dataSources)
        {
            if (shardingRouteContext == null)
            {
                return false;
            }

            if (dataSources.IsEmpty())
                return false;
            if (!shardingRouteContext.MustDataSource.TryGetValue(entityType,out HashSet<string> mustDataSources))
            {
                mustDataSources = new HashSet<string>();
                shardingRouteContext.MustDataSource.Add(entityType, mustDataSources);
            }

            return dataSources.Select(o => mustDataSources.Add(o)).All(o => o);
        }
        public static bool TryCreateOrAddAllMustDataSource(this ShardingRouteContext shardingRouteContext, params string[] dataSources)
        {
            if (shardingRouteContext == null)
            {
                return false;
            }

            if (dataSources.IsEmpty())
                return false;

            return dataSources.Select(o => shardingRouteContext.MustAllDataSource.Add(o)).All(o => o);
        }
        /// <summary>
        /// 创建或者添加提示路由
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="shardingRouteContext"></param>
        /// <param name="dataSources"></param>
        /// <returns>任何一个dataSources被添加成功就返回成功</returns>
        public static bool TryCreateOrAddHintDataSource<TEntity>(this ShardingRouteContext shardingRouteContext, params string[] dataSources) where TEntity : class
        {
            return TryCreateOrAddHintDataSource(shardingRouteContext, typeof(TEntity), dataSources);
        }
        /// <summary>
        /// 创建或者添加提示路由
        /// </summary>
        /// <param name="shardingRouteContext"></param>
        /// <param name="entityType"></param>
        /// <param name="dataSources"></param>
        /// <returns>任何一个dataSources被添加成功就返回成功</returns>
        public static bool TryCreateOrAddHintDataSource(this ShardingRouteContext shardingRouteContext, Type entityType, params string[] dataSources)
        {
            if (shardingRouteContext == null)
            {
                return false;
            }

            if (dataSources.IsEmpty())
                return false;
            if (!shardingRouteContext.HintDataSource.TryGetValue(entityType, out HashSet<string> hintDataSources))
            {
                hintDataSources = new HashSet<string>();
                shardingRouteContext.HintDataSource.Add(entityType, hintDataSources);
            }

            return dataSources.Select(o => hintDataSources.Add(o)).All(o => o);
        }
        public static bool TryCreateOrAddAllHintDataSource(this ShardingRouteContext shardingRouteContext,params string[] dataSources)
        {
            if (shardingRouteContext == null)
            {
                return false;
            }

            if (dataSources.IsEmpty())
                return false;
            return dataSources.Select(o => shardingRouteContext.HintAllDataSource.Add(o)).All(o => o);
        }
        /// <summary>
        /// 创建或者添加断言
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="shardingRouteContext"></param>
        /// <param name="dataSources"></param>
        /// <returns></returns>
        public static bool TryCreateOrAddAssertDataSource<TEntity>(this ShardingRouteContext shardingRouteContext, params IDataSourceRouteAssert<TEntity>[] dataSources) where TEntity : class
        {
            return TryCreateOrAddAssertDataSource(shardingRouteContext, typeof(TEntity), dataSources);
        }
        public static bool TryCreateOrAddAssertDataSource(this ShardingRouteContext shardingRouteContext, Type entityType, params IDataSourceRouteAssert[] asserts)
        {
            if (shardingRouteContext == null)
            {
                return false;
            }

            if (asserts.IsEmpty())
                return false;
            if (!shardingRouteContext.AssertDataSource.TryGetValue(entityType, out LinkedList<IDataSourceRouteAssert> routeAsserts))
            { 
                routeAsserts = new LinkedList<IDataSourceRouteAssert>();
                shardingRouteContext.AssertDataSource.Add(entityType, routeAsserts);
            }
            foreach (var routeAssert in asserts)
            {
                routeAsserts.AddLast(routeAssert);
            }

            return true;
        }
        public static bool TryCreateOrAddAssertAllDataSource(this ShardingRouteContext shardingRouteContext,params IDataSourceRouteAssert[] asserts)
        {
            if (shardingRouteContext == null)
            {
                return false;
            }

            if (asserts.IsEmpty())
                return false;
            foreach (var routeAssert in asserts)
            {
                shardingRouteContext.AssertAllDataSource.AddLast(routeAssert);
            }

            return true;
        }



        public static bool TryGetMustDataSource<TEntity>(this ShardingRouteContext shardingRouteContext, out HashSet<string> dataSources) where TEntity  : class
        {
            return TryGetMustDataSource(shardingRouteContext,typeof(TEntity),out dataSources);
        }
        public static bool TryGetMustDataSource(this ShardingRouteContext shardingRouteContext,Type entityType, out HashSet<string> dataSources)
        {
            if (shardingRouteContext == null)
            {
                dataSources = null;
                return false;
            }
            if (!shardingRouteContext.MustDataSource.ContainsKey(entityType))
            {
                dataSources = null;
                return false;
            }

            dataSources = shardingRouteContext.MustDataSource[entityType];
            return true;
        }
        public static bool TryGetHintDataSource<TEntity>(this ShardingRouteContext shardingRouteContext, out HashSet<string> dataSources) where TEntity  : class
        {
            return TryGetHintDataSource(shardingRouteContext,typeof(TEntity),out dataSources);
        }
        public static bool TryGetHintDataSource(this ShardingRouteContext shardingRouteContext,Type entityType, out HashSet<string> dataSources)
        {
            if (shardingRouteContext == null)
            {
                dataSources = null;
                return false;
            }
            if (!shardingRouteContext.HintDataSource.ContainsKey(entityType))
            {
                dataSources = null;
                return false;
            }

            dataSources = shardingRouteContext.HintDataSource[entityType];
            return true;
        }

        public static bool TryGetAssertDataSource<TEntity>(this ShardingRouteContext shardingRouteContext, out ICollection<IDataSourceRouteAssert> dataSources)where TEntity  : class
        {
            return TryGetAssertDataSource(shardingRouteContext,typeof(TEntity), out dataSources);
        }
        public static bool TryGetAssertDataSource(this ShardingRouteContext shardingRouteContext,Type entityType, out ICollection<IDataSourceRouteAssert> dataSources)
        {
            if (shardingRouteContext == null)
            {
                dataSources = null;
                return false;
            }
            if (!shardingRouteContext.AssertDataSource.ContainsKey(entityType))
            {
                dataSources = null;
                return false;
            }

            dataSources = shardingRouteContext.AssertDataSource[entityType];
            return true;
        }
        
    }
}
