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
    public interface IShardingConnectionStringResolver
    {
        bool ContainsReadWriteDataSourceName(string dataSourceName);
        string GetConnectionString(string dataSourceName);
        /// <summary>
        /// 添加数据源从库读字符串
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        bool AddConnectionString(string dataSourceName, string connectionString);
    }
}
