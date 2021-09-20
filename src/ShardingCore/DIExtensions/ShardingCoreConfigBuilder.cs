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
    public class ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext>
        where TActualDbContext : DbContext
        where TShardingDbContext : DbContext, IShardingDbContext<TActualDbContext>
    {
        public IServiceCollection Services { get; }


        public ShardingConfigOption<TShardingDbContext> ShardingConfigOption { get; }



        public ShardingCoreConfigBuilder(IServiceCollection services)
        {
            Services = services;
            ShardingConfigOption = new ShardingConfigOption<TShardingDbContext>();
        }


        public ShardingQueryBuilder<TShardingDbContext, TActualDbContext> Begin(bool ensureCreatedWithOutShardingTable, bool? createShardingTableOnStart = null,bool? ignoreCreateTableError = null)
        {
            ShardingConfigOption.EnsureCreatedWithOutShardingTable = ensureCreatedWithOutShardingTable;
            ShardingConfigOption.CreateShardingTableOnStart = createShardingTableOnStart;
            ShardingConfigOption.IgnoreCreateTableError = ignoreCreateTableError;
            return new ShardingQueryBuilder<TShardingDbContext, TActualDbContext>(this);
        }
        //public ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext> AddDefaultDataSource(string dataSourceName, string connectionString)
        //{
        //    if (!string.IsNullOrWhiteSpace(defaultDataSourceName) || !string.IsNullOrWhiteSpace(defaultConnectionString))
        //        throw new InvalidOperationException($"{nameof(AddDefaultDataSource)}-{dataSourceName}");
        //    this.defaultDataSourceName = dataSourceName;
        //    this.defaultConnectionString = connectionString;
        //    return this;
        //}

        //public ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext> AddDataSource(string dataSourceName, string connectionString)
        //{
        //    if (_dataSources.ContainsKey(dataSourceName))
        //        throw new InvalidOperationException($"{nameof(AddDataSource)}-{dataSourceName} repeat");
        //    _dataSources.Add(dataSourceName, connectionString);
        //    return this;
        //}
    }
}
