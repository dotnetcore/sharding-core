using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/18 17:27:44
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ReadWriteLoopConnector
    {
        private readonly string[] _connectionStrings;
        private readonly int _length;
        private long _seed = 0;
        public ReadWriteLoopConnector(string dataSourceName, IEnumerable<string> connectionStrings)
        {
            DataSourceName = dataSourceName;
            _connectionStrings = connectionStrings.ToArray();
            _length = _connectionStrings.Length;
        }

        public string DataSourceName { get; }

        public string GetConnectionString()
        {
            Interlocked.Increment(ref _seed);
            var next = (int)(_seed % _length);
            if (next < 0)
                return _connectionStrings[Math.Abs(next)];
            return _connectionStrings[next];
        }
    }
}
