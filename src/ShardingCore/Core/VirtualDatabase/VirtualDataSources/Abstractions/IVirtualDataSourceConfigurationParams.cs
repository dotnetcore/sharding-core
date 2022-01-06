using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ShardingComparision.Abstractions;
using System.Collections.Generic;
using System.Data.Common;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions
{
    public interface IVirtualDataSourceConfigurationParams
    {
        /// <summary>
        /// 配置id
        /// </summary>
        string ConfigId { get; }
        /// <summary>
        /// 优先级
        /// </summary>
        int Priority { get; }
        int MaxQueryConnectionsLimit { get; }
        ConnectionModeEnum ConnectionMode { get; }

        /// <summary>
        /// 默认数据源
        /// </summary>
        string DefaultDataSourceName { get; }
        /// <summary>
        /// 默认数据源链接字符串
        /// </summary>
        string DefaultConnectionString { get; }
        IDictionary<string,string> ExtraDataSources { get; }
        IDictionary<string, IEnumerable<string>> ReadWriteSeparationConfigs { get; }

        ReadStrategyEnum? ReadStrategy { get; }
        bool? ReadWriteDefaultEnable { get; }
        int? ReadWriteDefaultPriority { get; }
        /// <summary>
        /// 读写分离链接字符串获取
        /// </summary>
        ReadConnStringGetStrategyEnum? ReadConnStringGetStrategy { get; }
        IShardingComparer ShardingComparer { get; }
        /// <summary>
        /// 如何根据connectionString 配置 DbContextOptionsBuilder
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="dbContextOptionsBuilder"></param>
        /// <returns></returns>
        DbContextOptionsBuilder UseDbContextOptionsBuilder(string connectionString, DbContextOptionsBuilder dbContextOptionsBuilder);
        /// <summary>
        /// 如何根据dbConnection 配置DbContextOptionsBuilder
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="dbContextOptionsBuilder"></param>
        /// <returns></returns>
        DbContextOptionsBuilder UseDbContextOptionsBuilder(DbConnection dbConnection, DbContextOptionsBuilder dbContextOptionsBuilder);

        bool UseReadWriteSeparation();
    }

    public interface IVirtualDataSourceConfigurationParams<TShardingDbContext> : IVirtualDataSourceConfigurationParams
        where TShardingDbContext : DbContext, IShardingDbContext
    {

    }
}
