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
        /// <returns></returns>
        public string GetConnectionString();
        /// <summary>
        /// 添加链接字符串
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public bool AddConnectionString(string connectionString);
    }
}
