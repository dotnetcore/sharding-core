using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.Sharding.ShardingComparision;
using ShardingCore.Sharding.ShardingComparision.Abstractions;
using ShardingCore.TableExists;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/16 15:18:37
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingConfigOption<TShardingDbContext> : IShardingConfigOption<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly Dictionary<Type, Type> _virtualDataSourceRoutes = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Type> _virtualTableRoutes = new Dictionary<Type, Type>();
        private readonly ISet<Type> _createTableEntities = new HashSet<Type>();
        public readonly ISet<ParallelTableGroupNode> _parallelTables = new HashSet<ParallelTableGroupNode>();

        public Action<DbConnection, DbContextOptionsBuilder> SameConnectionConfigure { get; private set; }
        public Action<string, DbContextOptionsBuilder> DefaultQueryConfigure { get; private set; }

        public Func<IServiceProvider, IDictionary<string, string>> DataSourcesConfigure { get; private set; }

        public void UseShardingQuery(Action<string, DbContextOptionsBuilder> queryConfigure)
        {
            DefaultQueryConfigure = queryConfigure ?? throw new ArgumentNullException(nameof(queryConfigure));
        }
        public void UseShardingTransaction(Action<DbConnection, DbContextOptionsBuilder> transactionConfigure)
        {
            SameConnectionConfigure = transactionConfigure ?? throw new ArgumentNullException(nameof(transactionConfigure));
        }

        public void AddShardingDataSource(Func<IServiceProvider, IDictionary<string, string>> dataSourcesConfigure)
        {
            DataSourcesConfigure = dataSourcesConfigure ?? throw new ArgumentNullException(nameof(dataSourcesConfigure));
        }

        public Func<IServiceProvider, IShardingComparer<TShardingDbContext>> ReplaceShardingComparerFactory { get; private set; } = sp => new CSharpLanguageShardingComparer<TShardingDbContext>();
        /// <summary>
        /// 替换默认的比较器
        /// </summary>
        /// <param name="newShardingComparerFactory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ReplaceShardingComparer(Func<IServiceProvider, IShardingComparer<TShardingDbContext>> newShardingComparerFactory)
        {
            ReplaceShardingComparerFactory=newShardingComparerFactory ?? throw new ArgumentNullException(nameof(newShardingComparerFactory));
        }

        public Func<IServiceProvider, ITableEnsureManager<TShardingDbContext>> TableEnsureManagerFactory { get; private set; } = sp => new EmptyTableEnsureManager< TShardingDbContext>();
        public void AddTableEnsureManager(Func<IServiceProvider, ITableEnsureManager<TShardingDbContext>> newTableEnsureManagerFactory)
        {
            TableEnsureManagerFactory= newTableEnsureManagerFactory?? throw new ArgumentNullException(nameof(newTableEnsureManagerFactory));
        }


        ///// <summary>
        ///// 配置数据库分表查询和保存时的DbContext创建方式
        ///// </summary>
        ///// <param name="sameConnectionConfigure">DbConnection下如何配置因为不同的DbContext支持事务需要使用同一个DbConnection</param>
        ///// <param name="defaultQueryConfigure">默认查询DbContext创建的配置</param>

        //public void UseShardingOptionsBuilder(Action<DbConnection, DbContextOptionsBuilder> sameConnectionConfigure, Action<string,DbContextOptionsBuilder> defaultQueryConfigure = null)
        //{
        //    SameConnectionConfigure = sameConnectionConfigure ?? throw new ArgumentNullException(nameof(sameConnectionConfigure));
        //    DefaultQueryConfigure = defaultQueryConfigure ?? throw new ArgumentNullException(nameof(defaultQueryConfigure));
        //}

        public bool UseReadWrite => ReadConnStringConfigure != null;
        public Func<IServiceProvider, IDictionary<string, IEnumerable<string>>> ReadConnStringConfigure { get; private set; }
        public ReadStrategyEnum ReadStrategyEnum { get; private set; }
        public bool ReadWriteDefaultEnable { get; private set; }
        public int ReadWriteDefaultPriority { get; private set; }
        public ReadConnStringGetStrategyEnum ReadConnStringGetStrategy { get; private set; }

        /// <summary>
        /// 使用读写分离配置
        /// </summary>
        /// <param name="readConnStringConfigure"></param>
        /// <param name="readStrategyEnum"></param>
        /// <param name="defaultEnable">考虑到很多时候读写分离的延迟需要马上用到写入的数据所以默认关闭需要的话自己开启或者通过IShardingReadWriteManager,false表示默认不走读写分离除非你自己开启,true表示默认走读写分离除非你禁用,</param>
        /// <param name="defaultPriority">IShardingReadWriteManager.CreateScope()会判断dbcontext的priority然后判断是否启用readwrite</param>
        /// <param name="readConnStringGetStrategy">读写分离可能会造成每次查询不一样甚至分表后的分页会有错位问题，因为他不是一个原子操作,所以如果整个请求为一次读写切换大多数更加合适</param>
        public void UseReadWriteConfiguration(Func<IServiceProvider, IDictionary<string, IEnumerable<string>>> readConnStringConfigure, ReadStrategyEnum readStrategyEnum, bool defaultEnable = false, int defaultPriority = 10, ReadConnStringGetStrategyEnum readConnStringGetStrategy = ReadConnStringGetStrategyEnum.LatestFirstTime)
        {
            ReadConnStringConfigure = readConnStringConfigure ?? throw new ArgumentNullException(nameof(readConnStringConfigure));
            ReadStrategyEnum = readStrategyEnum;
            ReadWriteDefaultEnable = defaultEnable;
            ReadWriteDefaultPriority = defaultPriority;
            ReadConnStringGetStrategy = readConnStringGetStrategy;
        }


        public Type ShardingDbContextType => typeof(TShardingDbContext);

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

        public IDictionary<string, string> GetDataSources()
        {
            var defaultDataSources = new Dictionary<string, string>(){{DefaultDataSourceName,DefaultConnectionString}};
            return defaultDataSources.Concat(DataSourcesConfigure?.Invoke(ShardingContainer.ServiceProvider)??new Dictionary<string, string>()).ToDictionary(o=>o.Key,o=>o.Value);
        }


        /// <summary>
        /// 如果数据库不存在就创建并且创建表除了分表的
        /// </summary>
        public bool EnsureCreatedWithOutShardingTable { get; set; }

        /// <summary>
        /// 是否需要在启动时创建分表
        /// </summary>
        public bool? CreateShardingTableOnStart { get; set; }

        public bool AddEntityTryCreateTable(Type entityType)
        {
            return _createTableEntities.Add(entityType);
        }

        public bool AnyEntityTryCreateTable()
        {
            return _createTableEntities.Any();
        }

        public bool NeedCreateTable(Type entityType)
        {
            return _createTableEntities.Contains(entityType);
        }

        /// <summary>
        /// 忽略建表时的错误
        /// </summary>
        public bool? IgnoreCreateTableError { get; set; } = true;
        /// <summary>
        /// 自动追踪实体
        /// </summary>
        public bool AutoTrackEntity { get; set; }

        public string DefaultDataSourceName { get; set; }
        public string DefaultConnectionString { get; set; }
        public int MaxQueryConnectionsLimit { get; set; } = Environment.ProcessorCount;
        public ConnectionModeEnum ConnectionMode { get; set; } = ConnectionModeEnum.SYSTEM_AUTO;


        public bool AddParallelTableGroupNode(ParallelTableGroupNode parallelTableGroupNode)
        {
            Check.NotNull(parallelTableGroupNode, $"{nameof(parallelTableGroupNode)}");
            return _parallelTables.Add(parallelTableGroupNode);
        }
        public ISet<ParallelTableGroupNode> GetParallelTableGroupNodes()
        {
            return _parallelTables;
        }
        //public bool? EnableTableRouteCompileCache { get; set; }
        //public bool? EnableDataSourceRouteCompileCache { get; set; }
    }
}