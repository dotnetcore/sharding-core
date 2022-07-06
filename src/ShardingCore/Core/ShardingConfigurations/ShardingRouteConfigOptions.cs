using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ParallelTables;

namespace ShardingCore.Core.ShardingConfigurations
{
    public class ShardingRouteConfigOptions : IShardingRouteConfigOptions
    {
        private readonly IDictionary<Type, Type> _virtualDataSourceRoutes = new Dictionary<Type, Type>();
        private readonly IDictionary<Type, Type> _virtualTableRoutes = new Dictionary<Type, Type>();
        private readonly ISet<ParallelTableGroupNode> _parallelTables = new HashSet<ParallelTableGroupNode>();


        /// <summary>
        /// 添加分库路由
        /// </summary>
        /// <typeparam name="TRoute"></typeparam>
        public  void AddShardingDataSourceRoute<TRoute>() where TRoute : IVirtualDataSourceRoute
        {
            var routeType = typeof(TRoute);
            AddShardingDataSourceRoute(routeType);
        }
        public void AddShardingDataSourceRoute(Type routeType)
        {
            if (!routeType.IsVirtualDataSourceRoute())
                throw new ShardingCoreInvalidOperationException(routeType.FullName);
            //获取类型
            var genericVirtualRoute = routeType.GetInterfaces().FirstOrDefault(it => it.IsInterface && it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IVirtualDataSourceRoute<>)
                                                                                     && it.GetGenericArguments().Any());
            if (genericVirtualRoute == null)
                throw new ArgumentException($"add sharding route type error not assignable from {nameof(IVirtualDataSourceRoute<object>)}.");

            var shardingEntityType = genericVirtualRoute.GetGenericArguments()[0];
            if (shardingEntityType == null)
                throw new ArgumentException($"add sharding table route type error not assignable from {nameof(IVirtualDataSourceRoute<object>)}.");
            if (!_virtualDataSourceRoutes.ContainsKey(shardingEntityType))
            {
                _virtualDataSourceRoutes.Add(shardingEntityType, routeType);
            }
        }
        /// <summary>
        /// 添加分表路由
        /// </summary>
        /// <typeparam name="TRoute"></typeparam>
        public  void AddShardingTableRoute<TRoute>() where TRoute : IVirtualTableRoute
        {
            var routeType = typeof(TRoute);
            AddShardingTableRoute(routeType);
        }
        public void AddShardingTableRoute(Type routeType)
        {
            if (!routeType.IsIVirtualTableRoute())
                throw new ShardingCoreInvalidOperationException(routeType.FullName);
            //获取类型
            var genericVirtualRoute = routeType.GetInterfaces().FirstOrDefault(it => it.IsInterface && it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IVirtualTableRoute<>)
                                                                                     && it.GetGenericArguments().Any());
            if (genericVirtualRoute == null)
                throw new ArgumentException($"add sharding route type error not assignable from {nameof(IVirtualTableRoute<object>)}.");

            var shardingEntityType = genericVirtualRoute.GetGenericArguments()[0];
            if (shardingEntityType == null)
                throw new ArgumentException($"add sharding table route type error not assignable from {nameof(IVirtualTableRoute<object>)}.");
            if (!_virtualTableRoutes.ContainsKey(shardingEntityType))
            {
                _virtualTableRoutes.Add(shardingEntityType, routeType);
            }
        }

        public bool HasVirtualTableRoute(Type entityType)
        {
            return _virtualTableRoutes.ContainsKey(entityType);
        }

        public Type GetVirtualTableRouteType(Type entityType)
        {
            if (!_virtualTableRoutes.ContainsKey(entityType))
                throw new ArgumentException($"{entityType} not found {nameof(IVirtualTableRoute)}");
            return _virtualTableRoutes[entityType];
        }

        public bool HasVirtualDataSourceRoute(Type entityType)
        {
            return _virtualDataSourceRoutes.ContainsKey(entityType);
        }

        public Type GetVirtualDataSourceRouteType(Type entityType)
        {
            if (!_virtualDataSourceRoutes.ContainsKey(entityType))
                throw new ArgumentException($"{entityType} not found {nameof(IVirtualDataSourceRoute)}");
            return _virtualDataSourceRoutes[entityType];
        }

        public ISet<Type> GetShardingTableRouteTypes()
        {
            return _virtualTableRoutes.Keys.ToHashSet();
        }

        public ISet<Type> GetShardingDataSourceRouteTypes()
        {
            return _virtualDataSourceRoutes.Keys.ToHashSet();
        }
        public bool AddParallelTableGroupNode(ParallelTableGroupNode parallelTableGroupNode)
        {
            Check.NotNull(parallelTableGroupNode, $"{nameof(parallelTableGroupNode)}");
            return _parallelTables.Add(parallelTableGroupNode);
        }
        /// <summary>
        /// 获取平行表
        /// </summary>
        /// <returns></returns>
        public ISet<ParallelTableGroupNode> GetParallelTableGroupNodes()
        {
            return _parallelTables;
        }
    }
}
