using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
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

        Type GetVirtualTableRouteType(Type entityType);
        Type GetVirtualDataSourceRouteType(Type entityType);


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
        /// 忽略建表时的错误
        /// </summary>
        public bool? IgnoreCreateTableError { get; set; }

        public string DefaultDataSourceName { get; set; }
        public string DefaultConnectionString { get; set; }
    }

    public interface IShardingConfigOption<TShardingDbContext>: IShardingConfigOption where TShardingDbContext : DbContext, IShardingDbContext
    {

    }
}
