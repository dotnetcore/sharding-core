using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShardingCore.Exceptions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.ReadWriteConfigurations.Connectors.Abstractions;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/18 20:58:42
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ReadWriteRandomConnector:AbstractionReadWriteConnector
    {
        public ReadWriteRandomConnector(string dataSourceName,ReadNode[] readNodes):base(dataSourceName, readNodes)
        {
        }

        private string DoGetNoReadNameConnectionString()
        {
            if (Length == 1)
                return ReadNodes[0].ConnectionString;
            var next = RandomHelper.Next(0, Length);
            return ReadNodes[next].ConnectionString;
        }

        public override string DoGetConnectionString(string readNodeName)
        {
            if (readNodeName == null)
            {
                return DoGetNoReadNameConnectionString();
            }else
            {
                return ReadNodes.FirstOrDefault(o => o.Name == readNodeName)?.ConnectionString ??
                    throw new ShardingCoreInvalidOperationException($"read node name :[{readNodeName}] not found");
            }
        }
    }
}
