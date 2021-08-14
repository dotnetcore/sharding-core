using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.DbContexts.ShardingDbContexts;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/3 16:30:21
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingConfigEntry
    {
        public ShardingConfigEntry(Func<ShardingDbContextOptions, DbContext> creator, Type dbContextType, Action<ShardingDbConfigOptions> shardingDbConfigConfigure)
        {
            Creator = creator;
            DbContextType = dbContextType;
            DbConfigOptions = new ShardingDbConfigOptions();
            shardingDbConfigConfigure?.Invoke(DbConfigOptions);
        }

        public Type DbContextType { get; }
        public Func<ShardingDbContextOptions, DbContext> Creator { get; }
        public ShardingDbConfigOptions DbConfigOptions { get; }


        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
