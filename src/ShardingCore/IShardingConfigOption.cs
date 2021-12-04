using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/20 6:56:49
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IShardingConfigOption
    {
        Type ShardingDbContextType { get;}
        bool UseReadWrite { get; }

        bool HasVirtualTableRoute(Type entityType);
        Type GetVirtualTableRouteType(Type entityType);
        bool HasVirtualDataSourceRoute(Type entityType);
        Type GetVirtualDataSourceRouteType(Type entityType);

        ISet<Type> GetShardingTableRouteTypes();
        ISet<Type> GetShardingDataSourceRouteTypes();


        IDictionary<string, string> GetDataSources();


        /// <summary>
        /// 如果数据库不存在就创建并且创建表除了分表的
        /// </summary>
        public bool EnsureCreatedWithOutShardingTable { get; set; }
        /// <summary>
        /// 是否需要在启动时创建分表
        /// </summary>
        public bool? CreateShardingTableOnStart { get; set; }
        /// <summary>
        /// 添加尝试建表的对象
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public bool AddEntityTryCreateTable(Type entityType);
        public bool AnyEntityTryCreateTable();
        /// <summary>
        /// 是否需要启动创建表
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public bool NeedCreateTable(Type entityType);
        /// <summary>
        /// 忽略建表时的错误
        /// </summary>
        public bool? IgnoreCreateTableError { get; set; }

        /// <summary>
        /// 自动追踪实体
        /// </summary>
        public bool AutoTrackEntity { get; set; }

        /// <summary>
        /// 单次查询并发线程数目(1-65536)
        /// </summary>
        public int ParallelQueryMaxThreadCount { get; set; }
        /// <summary>
        /// 并发查询超时时间
        /// </summary>
        public TimeSpan ParallelQueryTimeOut { get; set; }
        /// <summary>
        /// 默认数据源名称
        /// </summary>
        public string DefaultDataSourceName { get; set; }
        /// <summary>
        /// 默认数据库链接字符串
        /// </summary>
        public string DefaultConnectionString { get; set; }
        /// <summary>
        /// 最大查询连接数限制
        /// </summary>
        public int MaxQueryConnectionsLimit { get; set; }
        /// <summary>
        /// 连接数限制
        /// </summary>
        public ConnectionModeEnum ConnectionMode { get; set; }
        /// <summary>
        /// 当ConnectionMode == SYSTEM_AUTO时生效
        /// </summary>
        public int UseMemoryLimitWhileSkip { get; set; }
    }

    public interface IShardingConfigOption<TShardingDbContext>: IShardingConfigOption where TShardingDbContext : DbContext, IShardingDbContext
    {

    }
}
