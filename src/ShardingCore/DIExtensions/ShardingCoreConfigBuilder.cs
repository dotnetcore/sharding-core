using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.DIExtensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/19 20:49:03
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingCoreConfigBuilder<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        public IServiceCollection Services { get; }


        public ShardingConfigOption<TShardingDbContext> ShardingConfigOption { get; }



        public ShardingCoreConfigBuilder(IServiceCollection services,Action<string,DbContextOptionsBuilder> queryConfigure)
        {
            Services = services;
            ShardingConfigOption = new ShardingConfigOption<TShardingDbContext>();
            ShardingConfigOption.UseShardingQuery(queryConfigure);
        }


        public ShardingTransactionBuilder<TShardingDbContext> Begin(Action<ShardingCoreBeginOptions> shardingCoreBeginOptionsConfigure)
        {
            var shardingCoreBeginOptions = new ShardingCoreBeginOptions();
            shardingCoreBeginOptionsConfigure?.Invoke(shardingCoreBeginOptions);
            if (shardingCoreBeginOptions.ParallelQueryMaxThreadCount <= 0)
                throw new ArgumentException(
                    $"{nameof(shardingCoreBeginOptions.ParallelQueryMaxThreadCount)} should greater than zero thread count");
            if (shardingCoreBeginOptions.ParallelQueryTimeOut.TotalMilliseconds <= 0)
                throw new ArgumentException(
                    $"{nameof(shardingCoreBeginOptions.ParallelQueryTimeOut)} should greater than zero milliseconds");
            ShardingConfigOption.EnsureCreatedWithOutShardingTable = shardingCoreBeginOptions.EnsureCreatedWithOutShardingTable;
            ShardingConfigOption.AutoTrackEntity = shardingCoreBeginOptions.AutoTrackEntity;
            ShardingConfigOption.ParallelQueryMaxThreadCount = shardingCoreBeginOptions.ParallelQueryMaxThreadCount;
            ShardingConfigOption.ParallelQueryTimeOut = shardingCoreBeginOptions.ParallelQueryTimeOut;
            ShardingConfigOption.CreateShardingTableOnStart = shardingCoreBeginOptions.CreateShardingTableOnStart;
            ShardingConfigOption.IgnoreCreateTableError = shardingCoreBeginOptions.IgnoreCreateTableError;
            foreach (var entityType in shardingCoreBeginOptions.GetCreateTableEntities())
            {
                ShardingConfigOption.AddEntityTryCreateTable(entityType);
            }

            return new ShardingTransactionBuilder<TShardingDbContext>(this);
            //return new ShardingQueryBuilder<TShardingDbContext>(this);
        }
        //public ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext> AddDefaultDataSource(string dataSourceName, string connectionString)
        //{
        //    if (!string.IsNullOrWhiteSpace(defaultDataSourceName) || !string.IsNullOrWhiteSpace(defaultConnectionString))
        //        throw new ShardingCoreInvalidOperationException($"{nameof(AddDefaultDataSource)}-{dataSourceName}");
        //    this.defaultDataSourceName = dataSourceName;
        //    this.defaultConnectionString = connectionString;
        //    return this;
        //}

        //public ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext> AddDataSource(string dataSourceName, string connectionString)
        //{
        //    if (_dataSources.ContainsKey(dataSourceName))
        //        throw new ShardingCoreInvalidOperationException($"{nameof(AddDataSource)}-{dataSourceName} repeat");
        //    _dataSources.Add(dataSourceName, connectionString);
        //    return this;
        //}
    }

    public class ShardingCoreBeginOptions
    {
        /// <summary>
        /// 如果数据库不存在就创建并且创建表除了分表的
        /// </summary>
        public bool EnsureCreatedWithOutShardingTable { get; set; }

        /// <summary>
        /// 是否需要在启动时创建分表
        /// </summary>
        public bool? CreateShardingTableOnStart { get; set; }
        /// <summary>
        /// 是否自动追踪实体
        /// 譬如本次查询涉及到a1,a2,a3这三张表，会创建3个dbcontext进行查询，如果AutoTrackEntity=false那么针对被创建的dbcontext不会有任何变化，还是以追踪的形式查询
        /// 因为会同时创建3个dbcontext所以针对跨表查询完成后dbcontext会被回收，但是查询还是按原先的行为查询，所以如果不启用建议在查询的时候使用notracking
        /// 如果AutoTrackEntity=true，那么被创建的三个dbcontext还是以原先的表现行为进行查询，在查询完成后会再次各自创建对应的dbcontext进行对象的追踪
        /// </summary>
        public bool AutoTrackEntity { get; set; }

        /// <summary>
        /// 单次查询并发线程数目(最小1)默认cpu核心数*2
        /// </summary>
        public int ParallelQueryMaxThreadCount { get; set; } = Environment.ProcessorCount*2;
        /// <summary>
        /// 默认30秒超时
        /// </summary>
        public TimeSpan ParallelQueryTimeOut { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 忽略建表时的错误
        /// </summary>
        public bool? IgnoreCreateTableError { get; set; } = true;

        private readonly  ISet<Type> _createTableEntities = new HashSet<Type>();

        public void AddEntitiesTryCreateTable(params Type[] entityTypes)
        {
            foreach (var entityType in entityTypes)
            {
                _createTableEntities.Add(entityType);
            }

        }

        public ISet<Type> GetCreateTableEntities()
        {
            return _createTableEntities;
        }
    }
}
