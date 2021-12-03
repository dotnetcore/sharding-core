using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ShardingCore.Sharding.ReadWriteConfigurations.Connectors.Abstractions;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/18 17:27:44
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ReadWriteLoopConnector: AbstractionReadWriteConnector
    {
        private long _seed = 0;
        public ReadWriteLoopConnector(string dataSourceName, IEnumerable<string> connectionStrings):base(dataSourceName,connectionStrings)
        {
        }

        public override string DoGetConnectionString()
        {
            if (Length == 1)
                return ConnectionStrings[0];
            var newValue = Interlocked.Increment(ref _seed);
            var next = (int)(newValue % Length);
            if (next < 0)
                return ConnectionStrings[Math.Abs(next)];
            return ConnectionStrings[next];
        }
    }
}
