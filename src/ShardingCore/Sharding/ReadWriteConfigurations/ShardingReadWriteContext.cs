using System;
using System.Collections.Generic;
using System.Text;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/6 16:52:29
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingReadWriteContext
    {
        public bool DefaultReadEnable { get; set; }
        public int DefaultPriority { get; set; }
        private readonly Dictionary<string /*数据源*/, string /*数据源对应的读节点名称*/> _dataSourceReadNode;

        private ShardingReadWriteContext()
        {
            DefaultReadEnable = false;
            DefaultPriority = 0;
            _dataSourceReadNode = new Dictionary<string, string>();
        }

        public static ShardingReadWriteContext Create()
        {
            return new ShardingReadWriteContext();
        }
        /// <summary>
        /// 添加数据源对应读节点获取名称
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="readNodeName"></param>
        /// <returns></returns>
        public bool AddDataSourceReadNode(string dataSource, string readNodeName)
        {
            if (_dataSourceReadNode.ContainsKey(dataSource))
            {
                return false;
            }
            _dataSourceReadNode.Add(dataSource, readNodeName);
            return true;
        }
        /// <summary>
        /// 尝试获取对应数据源的读节点名称
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="readNodeName"></param>
        /// <returns></returns>
        public bool TryGetDataSourceReadNode(string dataSource,out string readNodeName)
        {
            return _dataSourceReadNode.TryGetValue(dataSource, out readNodeName);
        }
    }
}
