using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.ReadWriteConfigurations.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/6 13:01:59
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 读写分离链接字符串解析
    /// </summary>
    /// <typeparam name="TShardingDbContext"></typeparam>
    public interface IShardingConnectionStringResolver<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        bool ContainsReadWriteDataSourceName(string dataSourceName);
        string GetConnectionString(string dataSourceName);
    }
}
