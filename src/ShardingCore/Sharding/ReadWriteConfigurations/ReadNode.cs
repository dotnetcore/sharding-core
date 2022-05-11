using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/11 8:31:38
    /// Email: 326308290@qq.com
    public class ReadNode
    {
        /// <summary>
        /// 当前读库节点名称
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// 当前读库链接的连接字符串
        /// </summary>
        public string ConnectionString { get; }

        public ReadNode(string name,string connectionString)
        {
            Name = name??throw new ArgumentNullException("read node name is null");
            ConnectionString = connectionString;
        }
    }
}
