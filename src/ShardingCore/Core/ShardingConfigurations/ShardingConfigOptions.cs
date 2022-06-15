using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ShardingComparision;
using ShardingCore.Sharding.ShardingComparision.Abstractions;
using ShardingCore.TableExists;
using ShardingCore.TableExists.Abstractions;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace ShardingCore.Core.ShardingConfigurations
{
    public class ShardingConfigOptions<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        /// <summary>
        /// 配置id,如果是单配置可以用guid代替,如果是多配置该属性表示每个配置的id
        /// </summary>
        public string ConfigId { get; set; }
        /// <summary>
        /// 优先级多个配置之间的优先级
        /// </summary>
        public int Priority { get; set; }
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
        public Func<IServiceProvider, IDictionary<string, string>> DataSourcesConfigure { get; private set; }
        /// <summary>
        /// 添加额外数据源
        /// </summary>
        /// <param name="extraDataSourceConfigure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddExtraDataSource(Func<IServiceProvider, IDictionary<string, string>> extraDataSourceConfigure)
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
            Func<IServiceProvider, IDictionary<string, IEnumerable<string>>> readWriteSeparationConfigure,
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
        public void AddReadWriteNodeSeparation(
            Func<IServiceProvider, IDictionary<string, IEnumerable<ReadNode>>> readWriteNodeSeparationConfigure,
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
        public Func<IServiceProvider, IShardingComparer> ReplaceShardingComparerFactory { get; private set; } = sp => new CSharpLanguageShardingComparer();
        /// <summary>
        /// 替换默认的比较器
        /// </summary>
        /// <param name="newShardingComparerFactory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ReplaceShardingComparer(Func<IServiceProvider, IShardingComparer> newShardingComparerFactory)
        {
            ReplaceShardingComparerFactory = newShardingComparerFactory ?? throw new ArgumentNullException(nameof(newShardingComparerFactory));
        }

        public Func<IServiceProvider, ITableEnsureManager<TShardingDbContext>> TableEnsureManagerFactory =
            sp => new EmptyTableEnsureManager<TShardingDbContext>();

        public void ReplaceTableEnsureManager(
            Func<IServiceProvider, ITableEnsureManager<TShardingDbContext>> tableEnsureManagerConfigure)
        {
            TableEnsureManagerFactory = tableEnsureManagerConfigure ??
                                        throw new ArgumentNullException(nameof(tableEnsureManagerConfigure));
        }


    }
}
