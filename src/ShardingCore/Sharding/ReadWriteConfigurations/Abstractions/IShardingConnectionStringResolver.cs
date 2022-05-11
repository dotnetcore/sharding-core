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
        /// <summary>
        /// 获取指定数据源的读连接名称节点
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="readNodeName">名称不存在报错,如果为null那么就随机获取</param>
        /// <returns></returns>
        string GetConnectionString(string dataSourceName,string readNodeName);
        /// <summary>
        /// 添加数据源从库读字符串
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        /// <param name="readNodeName"></param>
        /// <returns></returns>
        bool AddConnectionString(string dataSourceName, string connectionString, string readNodeName);
    }
}
