using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.ReadWriteConfigurations.Abstractions
{
    public interface IReadWriteConnector
    {
        /// <summary>
        /// 数据源
        /// </summary>
        public string DataSourceName { get; }
        /// <summary>
        /// 获取链接字符串
        /// </summary>
        /// <param name="readNodeName">可为null</param>
        /// <returns></returns>
        public string GetConnectionString(string readNodeName);
        /// <summary>
        /// 添加链接字符串
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="readNodeName"></param>
        /// <returns></returns>
        public bool AddConnectionString(string connectionString, string readNodeName);
    }
}
