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
    public class ShardingEntityConfigOptions<TShardingDbContext> : IShardingEntityConfigOptions<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IDictionary<Type, Type> _virtualDataSourceRoutes = new Dictionary<Type, Type>();
        private readonly IDictionary<Type, Type> _virtualTableRoutes = new Dictionary<Type, Type>();
        private readonly ISet<ParallelTableGroupNode> _parallelTables = new HashSet<ParallelTableGroupNode>();

        /// <summary>
        /// 如果数据库不存在就创建并且创建表除了分表的
        /// </summary>
        public bool EnsureCreatedWithOutShardingTable { get; set; }

        /// <summary>
        /// 是否需要在启动时创建分表
        /// </summary>
        public bool? CreateShardingTableOnStart { get; set; }
        /// <summary>
        /// 是否在启动时创建数据库
        /// </summary>
        public bool? CreateDataBaseOnlyOnStart { get; set; }
        /// <summary>
        /// 当查询遇到没有路由被命中时是否抛出错误
        /// </summary>
        public bool ThrowIfQueryRouteNotMatch { get; set; } = true;
        ///// <summary>
        ///// 全局启用分表路由表达式缓存,仅缓存单个表达式
        ///// </summary>
        //public bool? EnableTableRouteCompileCache { get; set; }
        ///// <summary>
        ///// 全局启用分库路由表达式缓存,仅缓存单个表达式
        ///// </summary>
        //public bool? EnableDataSourceRouteCompileCache { get; set; }
        /// <summary>
        /// 忽略建表时的错误
        /// </summary>
        public bool? IgnoreCreateTableError { get; set; } = false;
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
        /// <summary>
        /// 多个DbContext事务传播委托
        /// </summary>
        public Action<DbConnection, DbContextOptionsBuilder> ConnectionConfigure { get; private set; }
        /// <summary>
        /// 初始DbContext的创建委托
        /// </summary>
        public Action<string, DbContextOptionsBuilder> ConnectionStringConfigure { get; private set; }
        /// <summary>
        /// 仅内部DbContext生效的配置委托
        /// </summary>
        public Action<DbContextOptionsBuilder> ExecutorDbContextConfigure { get; private set; }
        public Action<DbContextOptionsBuilder> ShellDbContextConfigure { get; private set; }


        /// <summary>
        /// 如何使用字符串创建DbContext
        /// </summary>
        public void UseShardingQuery(Action<string, DbContextOptionsBuilder> queryConfigure)
        {
            ConnectionStringConfigure = queryConfigure ?? throw new ArgumentNullException(nameof(queryConfigure));
        }
        /// <summary>
        /// 如何传递事务到其他DbContext
        /// </summary>
        public void UseShardingTransaction(Action<DbConnection, DbContextOptionsBuilder> transactionConfigure)
        {
            ConnectionConfigure = transactionConfigure ?? throw new ArgumentNullException(nameof(transactionConfigure));
        }
        /// <summary>
        /// 仅内部真实DbContext配置的方法
        /// </summary>
        /// <param name="executorDbContextConfigure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void UseExecutorDbContextConfigure(Action<DbContextOptionsBuilder> executorDbContextConfigure)
        {
            ExecutorDbContextConfigure = executorDbContextConfigure ?? throw new ArgumentNullException(nameof(executorDbContextConfigure));
        }

        public void UseShellDbContextConfigure(Action<DbContextOptionsBuilder> shellDbContextConfigure)
        {
            ShellDbContextConfigure = shellDbContextConfigure ?? throw new ArgumentNullException(nameof(shellDbContextConfigure));
        }
    }
}
