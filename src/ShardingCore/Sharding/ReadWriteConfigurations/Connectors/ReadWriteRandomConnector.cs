using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public ReadWriteRandomConnector(string dataSourceName,IEnumerable<string> connectionStrings):base(dataSourceName,connectionStrings)
        {
        }

        public override string DoGetConnectionString()
        {
            if (Length == 1)
                return ConnectionStrings[0];
            var next = RandomHelper.Next(0, Length);
            return ConnectionStrings[next];
        }

    }
}
