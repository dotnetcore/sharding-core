using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.ReadWriteConfigurations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using ShardingCore.Core.ServiceProviders;

namespace ShardingCore.Core.ShardingConfigurations
{
    /// <summary>
    /// 分片配置
    /// </summary>
    public class ShardingConfigOptions
    {
        /// <summary>
        /// 写操作数据库后自动使用写库链接防止读库链接未同步无法查询到数据
        /// </summary>
        public bool AutoUseWriteConnectionStringAfterWriteDb { get; set; } = false;
        /// <summary>
        /// 当查询遇到没有路由被命中时是否抛出错误
        /// </summary>
        public bool ThrowIfQueryRouteNotMatch { get; set; } = true;

        /// <summary>
        /// 忽略建表时的错误
        /// </summary>
        public bool? IgnoreCreateTableError { get; set; } = false;
        /// <summary>
        /// 配置全局迁移最大并行数,以data source为一个单元并行迁移保证在多数据库分库情况下可以大大提高性能
        /// 默认系统逻辑处理器<code>Environment.ProcessorCount</code>
        /// </summary>
        public int MigrationParallelCount { get; set; }= Environment.ProcessorCount;
        /// <summary>
        /// 启动补偿表的最大并行数,以data source为一个单元并行迁移保证在多数据库分库情况下可以大大提高性能
        /// 默认系统逻辑处理器<code>Environment.ProcessorCount</code>
        /// </summary>
        public int CompensateTableParallelCount { get; set; }= Environment.ProcessorCount;
        /// <summary>
        /// 全局配置最大的查询连接数限制,默认系统逻辑处理器<code>Environment.ProcessorCount</code>
        /// </summary>
        public int MaxQueryConnectionsLimit { get; set; } = Environment.ProcessorCount;
        /// <summary>
        /// 默认<code>ConnectionModeEnum.SYSTEM_AUTO</code>
        /// </summary>
        public ConnectionModeEnum ConnectionMode { get; set; } = ConnectionModeEnum.SYSTEM_AUTO;
        /// <summary>
        /// 读写分离配置
        /// </summary>
        public ShardingReadWriteSeparationOptions ShardingReadWriteSeparationOptions { get; private set; }
        /// <summary>
        /// 默认数据源
        /// </summary>
        public string DefaultDataSourceName { get; private set; }
        /// <summary>
        /// 默认数据源链接字符串
        /// </summary>
        public string DefaultConnectionString { get; private set; }
        /// <summary>
        /// 添加默认数据源
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddDefaultDataSource(string dataSourceName, string connectionString)
        {
            DefaultDataSourceName= dataSourceName?? throw new ArgumentNullException(nameof(dataSourceName));
            DefaultConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }
        public Func<IShardingProvider, IDictionary<string, string>> DataSourcesConfigure { get; private set; }
        /// <summary>
        /// 添加额外数据源
        /// </summary>
        /// <param name="extraDataSourceConfigure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddExtraDataSource(Func<IShardingProvider, IDictionary<string, string>> extraDataSourceConfigure)
        {
            DataSourcesConfigure= extraDataSourceConfigure ?? throw new ArgumentNullException(nameof(extraDataSourceConfigure));
        }
        /// <summary>
        /// 添加读写分离配置
        /// </summary>
        /// <param name="readWriteSeparationConfigure"></param>
        /// <param name="readStrategyEnum">随机或者轮询</param>
        /// <param name="defaultEnable">false表示哪怕您添加了读写分离也不会进行读写分离查询,只有需要的时候自行开启,true表示默认查询就是走的读写分离</param>
        /// <param name="defaultPriority">默认优先级建议大于0</param>
        /// <param name="readConnStringGetStrategy">LatestFirstTime:DbContext缓存,LatestEveryTime:每次都是最新</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddReadWriteSeparation(
            Func<IShardingProvider, IDictionary<string, IEnumerable<string>>> readWriteSeparationConfigure,
            ReadStrategyEnum readStrategyEnum,
            bool defaultEnable = false,
            int defaultPriority = 10,
            ReadConnStringGetStrategyEnum readConnStringGetStrategy = ReadConnStringGetStrategyEnum.LatestFirstTime)
        {
            ShardingReadWriteSeparationOptions = new ShardingReadWriteSeparationOptions();
            ShardingReadWriteSeparationOptions.ReadWriteSeparationConfigure= readWriteSeparationConfigure ?? throw new ArgumentNullException(nameof(readWriteSeparationConfigure));
            ShardingReadWriteSeparationOptions.ReadStrategy = readStrategyEnum;
            ShardingReadWriteSeparationOptions.DefaultEnable=defaultEnable;
            ShardingReadWriteSeparationOptions.DefaultPriority= defaultPriority;
            ShardingReadWriteSeparationOptions.ReadConnStringGetStrategy= readConnStringGetStrategy;
        }
        /// <summary>
        /// 读写分离配置 和 AddReadWriteSeparation不同的是
        /// 当前配置支持自定义读链接节点命名,命名的好处在于当使用读库链接的时候由于服务器性能的差异
        /// 可以将部分吃性能的查询通过节点名称切换到对应的性能相对较好或者较空闲的读库服务器
        /// <code><![CDATA[
        /// IShardingReadWriteManager _readWriteManager=...
        ///   using (_readWriteManager.CreateScope())
        ///     {
        ///         _readWriteManager.GetCurrent().SetReadWriteSeparation(100,true);
        ///         _readWriteManager.GetCurrent().AddDataSourceReadNode("A", readNodeName);
        ///         var xxxaaa = await _defaultTableDbContext.Set<SysUserSalary>().FirstOrDefaultAsync();
        ///   }]]></code>
        /// </summary>
        /// <param name="readWriteNodeSeparationConfigure"></param>
        /// <param name="readStrategyEnum"></param>
        /// <param name="defaultEnable"></param>
        /// <param name="defaultPriority"></param>
        /// <param name="readConnStringGetStrategy"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddReadWriteNodeSeparation(
            Func<IShardingProvider, IDictionary<string, IEnumerable<ReadNode>>> readWriteNodeSeparationConfigure,
            ReadStrategyEnum readStrategyEnum,
            bool defaultEnable = false,
            int defaultPriority = 10,
            ReadConnStringGetStrategyEnum readConnStringGetStrategy = ReadConnStringGetStrategyEnum.LatestFirstTime)
        {
            ShardingReadWriteSeparationOptions = new ShardingReadWriteSeparationOptions();
            ShardingReadWriteSeparationOptions.ReadWriteNodeSeparationConfigure= readWriteNodeSeparationConfigure ?? throw new ArgumentNullException(nameof(readWriteNodeSeparationConfigure));
            ShardingReadWriteSeparationOptions.ReadStrategy = readStrategyEnum;
            ShardingReadWriteSeparationOptions.DefaultEnable=defaultEnable;
            ShardingReadWriteSeparationOptions.DefaultPriority= defaultPriority;
            ShardingReadWriteSeparationOptions.ReadConnStringGetStrategy= readConnStringGetStrategy;
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
        /// 外部dbcontext的配置委托
        /// </summary>
        public Action<DbContextOptionsBuilder> ShellDbContextConfigure { get; private set; }
        /// <summary>
        /// 仅内部真正执行的DbContext生效的配置委托
        /// </summary>
        public Action<DbContextOptionsBuilder> ExecutorDbContextConfigure { get; private set; }
        /// <summary>
        /// 分片迁移使用的配置
        /// </summary>
        
        public Action<DbContextOptionsBuilder> ShardingMigrationConfigure { get; private set; }

        /// <summary>
        /// 添加分片迁移的配置
        /// 当前配置只有在调用迁移代码时才会生效
        /// <code><![CDATA[
        /// using (var scope = app.ApplicationServices.CreateScope())
        /// {
        ///   var defaultShardingDbContext = scope.ServiceProvider.GetService<DefaultShardingDbContext>();
        ///    defaultShardingDbContext.Database.Migrate();
        /// }
        /// ]]></code>
        /// </summary>
        /// <param name="configure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void UseShardingMigrationConfigure(Action<DbContextOptionsBuilder> configure)
        {
            ShardingMigrationConfigure = configure ?? throw new ArgumentNullException(nameof(configure));
        }
        /// <summary>
        /// 如何使用字符串创建DbContext
        /// </summary>
        /// <param name="queryConfigure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void UseShardingQuery(Action<string, DbContextOptionsBuilder> queryConfigure)
        {
            ConnectionStringConfigure = queryConfigure ?? throw new ArgumentNullException(nameof(queryConfigure));
        }
        /// <summary>
        /// 如何传递事务到其他DbContext
        /// </summary>
        /// <param name="transactionConfigure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void UseShardingTransaction(Action<DbConnection, DbContextOptionsBuilder> transactionConfigure)
        {
            ConnectionConfigure = transactionConfigure ?? throw new ArgumentNullException(nameof(transactionConfigure));
        }
        /// <summary>
        /// 仅内部正真执行DbContext生效,作为最外面的壳DbContext将不会生效
        /// </summary>
        /// <param name="executorDbContextConfigure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void UseExecutorDbContextConfigure(Action<DbContextOptionsBuilder> executorDbContextConfigure)
        {
            ExecutorDbContextConfigure= executorDbContextConfigure ?? throw new ArgumentNullException(nameof(executorDbContextConfigure));
        }
        /// <summary>
        /// 仅外部DbContext生效,如果是独立调用AddDbContext和AddShardingConfigure不一定生效
        /// 会在AddShardingDbContext里面自动赋值
        /// </summary>
        /// <param name="shellDbContextConfigure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void UseShellDbContextConfigure(Action<DbContextOptionsBuilder> shellDbContextConfigure)
        {
            ShellDbContextConfigure = shellDbContextConfigure ?? throw new ArgumentNullException(nameof(shellDbContextConfigure));
        }

        // public Func<IServiceProvider, ITableEnsureManager<TShardingDbContext>> TableEnsureManagerFactory =
        //     sp => new EmptyTableEnsureManager<TShardingDbContext>();
        //
        // public void ReplaceTableEnsureManager(
        //     Func<IServiceProvider, ITableEnsureManager<TShardingDbContext>> tableEnsureManagerConfigure)
        // {
        //     TableEnsureManagerFactory = tableEnsureManagerConfigure ??
        //                                 throw new ArgumentNullException(nameof(tableEnsureManagerConfigure));
        // }

        public void CheckArguments()
        {
            if (string.IsNullOrWhiteSpace(DefaultDataSourceName))
                throw new ArgumentNullException(
                    $"{nameof(DefaultDataSourceName)} plz call {nameof(AddDefaultDataSource)}");
            
            if (string.IsNullOrWhiteSpace(DefaultConnectionString))
                throw new ArgumentNullException(
                    $"{nameof(DefaultConnectionString)} plz call {nameof(AddDefaultDataSource)}");

            if (ConnectionStringConfigure is null)
                throw new ArgumentNullException($"plz call {nameof(UseShardingQuery)}");
            if (ConnectionConfigure is null )
                throw new ArgumentNullException(
                    $"plz call {nameof(UseShardingTransaction)}");

            if (MaxQueryConnectionsLimit <= 0)
                throw new ArgumentException(
                    $"{nameof(MaxQueryConnectionsLimit)} should greater than and equal 1");
        }

    }
}
