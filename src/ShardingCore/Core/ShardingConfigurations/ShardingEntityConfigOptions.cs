using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ParallelTables;

namespace ShardingCore.Core.ShardingConfigurations
{
    public interface IShardingEntityConfigOptions
    {
        /// <summary>
        /// 如果数据库不存在就创建并且创建表除了分表的
        /// </summary>
        bool EnsureCreatedWithOutShardingTable { get; set; }

        /// <summary>
        /// 是否需要在启动时创建分表
        /// </summary>
        bool? CreateShardingTableOnStart { get; set; }
        /// <summary>
        /// 当查询遇到没有路由被命中时是否抛出错误
        /// </summary>
        bool ThrowIfQueryRouteNotMatch { get; set; }
        /// <summary>
        /// 忽略建表时的错误
        /// </summary>
        bool? IgnoreCreateTableError { get; set; }
        bool? EnableTableRouteCompileCache { get; set; }
        bool? EnableDataSourceRouteCompileCache { get; set; }
        /// <summary>
        /// 添加分表路由
        /// </summary>
        /// <typeparam name="TRoute"></typeparam>
        void AddShardingDataSourceRoute<TRoute>() where TRoute : IVirtualDataSourceRoute;
        void AddShardingDataSourceRoute(Type routeType);
        /// <summary>
        /// 添加分表路由
        /// </summary>
        /// <typeparam name="TRoute"></typeparam>
        void AddShardingTableRoute<TRoute>() where TRoute : IVirtualTableRoute;
        void AddShardingTableRoute(Type routeType);

        bool HasVirtualTableRoute(Type entityType);

        Type GetVirtualTableRouteType(Type entityType);

        bool HasVirtualDataSourceRoute(Type entityType);

        Type GetVirtualDataSourceRouteType(Type entityType);

        ISet<Type> GetShardingTableRouteTypes();

        ISet<Type> GetShardingDataSourceRouteTypes();

        /// <summary>
        /// 平行表
        /// </summary>
        bool AddParallelTableGroupNode(ParallelTableGroupNode parallelTableGroupNode);
        ISet<ParallelTableGroupNode> GetParallelTableGroupNodes();
    }
    public interface IShardingEntityConfigOptions<TShardingDbContext>: IShardingEntityConfigOptions where TShardingDbContext : DbContext, IShardingDbContext
    {
    }
    public class ShardingEntityConfigOptions<TShardingDbContext> : IShardingEntityConfigOptions<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly Dictionary<Type, Type> _virtualDataSourceRoutes = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Type> _virtualTableRoutes = new Dictionary<Type, Type>();
        public readonly ISet<ParallelTableGroupNode> _parallelTables = new HashSet<ParallelTableGroupNode>();

        /// <summary>
        /// 如果数据库不存在就创建并且创建表除了分表的
        /// </summary>
        public bool EnsureCreatedWithOutShardingTable { get; set; }

        /// <summary>
        /// 是否需要在启动时创建分表
        /// </summary>
        public bool? CreateShardingTableOnStart { get; set; }
        /// <summary>
        /// 当查询遇到没有路由被命中时是否抛出错误
        /// </summary>
        public bool ThrowIfQueryRouteNotMatch { get; set; } = true;
        public bool? EnableTableRouteCompileCache { get; set; }
        public bool? EnableDataSourceRouteCompileCache { get; set; }
        /// <summary>
        /// 忽略建表时的错误
        /// </summary>
        public bool? IgnoreCreateTableError { get; set; } = true;
        /// <summary>
        /// 添加分表路由
        /// </summary>
        /// <typeparam name="TRoute"></typeparam>
        public void AddShardingDataSourceRoute<TRoute>() where TRoute : IVirtualDataSourceRoute
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
                throw new ArgumentException("add sharding route type error not assignable from IVirtualDataSourceRoute<>.");

            var shardingEntityType = genericVirtualRoute.GetGenericArguments()[0];
            if (shardingEntityType == null)
                throw new ArgumentException("add sharding table route type error not assignable from IVirtualDataSourceRoute<>");
            if (!_virtualDataSourceRoutes.ContainsKey(shardingEntityType))
            {
                _virtualDataSourceRoutes.Add(shardingEntityType, routeType);
            }
        }
        /// <summary>
        /// 添加分表路由
        /// </summary>
        /// <typeparam name="TRoute"></typeparam>
        public void AddShardingTableRoute<TRoute>() where TRoute : IVirtualTableRoute
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
                throw new ArgumentException("add sharding route type error not assignable from IVirtualTableRoute<>.");

            var shardingEntityType = genericVirtualRoute.GetGenericArguments()[0];
            if (shardingEntityType == null)
                throw new ArgumentException("add sharding table route type error not assignable from IVirtualTableRoute<>");
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
                throw new ArgumentException($"{entityType} not found IVirtualTableRoute");
            return _virtualTableRoutes[entityType];
        }

        public bool HasVirtualDataSourceRoute(Type entityType)
        {
            return _virtualDataSourceRoutes.ContainsKey(entityType);
        }

        public Type GetVirtualDataSourceRouteType(Type entityType)
        {
            if (!_virtualDataSourceRoutes.ContainsKey(entityType))
                throw new ArgumentException($"{entityType} not found IVirtualDataSourceRoute");
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
        public ISet<ParallelTableGroupNode> GetParallelTableGroupNodes()
        {
            return _parallelTables;
        }
    }
}
