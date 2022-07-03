using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 05 February 2021 13:01:39
    * @Email: 326308290@qq.com
    */

    public interface IVirtualDataSource
    {
        /// <summary>
        /// 数据源配置
        /// </summary>
        IVirtualDataSourceConfigurationParams ConfigurationParams { get; }
        /// <summary>
        /// 链接字符串管理
        /// </summary>
        IConnectionStringManager ConnectionStringManager { get; }
        /// <summary>
        /// 是否启用了读写分离
        /// </summary>
        bool UseReadWriteSeparation { get; }
        /// <summary>
        /// 默认的数据源名称
        /// </summary>
        string DefaultDataSourceName { get; }
        /// <summary>
        /// 默认连接字符串
        /// </summary>
        string DefaultConnectionString { get;}
       

        /// <summary>
        /// 获取默认的数据源信息
        /// </summary>
        /// <returns></returns>
        IPhysicDataSource GetDefaultDataSource();
        /// <summary>
        /// 获取数据源
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <exception cref="ShardingCoreNotFoundException">
        ///     thrown if data source name is not in virtual data source
        ///     the length of the buffer
        /// </exception>
        /// <returns></returns>
        IPhysicDataSource GetPhysicDataSource(string dataSourceName);
        /// <summary>
        /// 获取所有的数据源名称
        /// </summary>
        /// <returns></returns>
        List<string> GetAllDataSourceNames();

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreNotFoundException"></exception>
        string GetConnectionString(string dataSourceName);

        /// <summary>
        /// 添加数据源
        /// </summary>
        /// <param name="physicDataSource"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreInvalidOperationException">重复添加默认数据源</exception>
        bool AddPhysicDataSource(IPhysicDataSource physicDataSource);

        /// <summary>
        /// 是否默认数据源
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        bool IsDefault(string dataSourceName);
        /// <summary>
        /// 检查是否配置默认数据源和默认链接字符串
        /// </summary>
        /// <exception cref="ShardingCoreInvalidOperationException"></exception>
        void CheckVirtualDataSource();
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

        IDictionary<string, string> GetDataSources();
    }
}