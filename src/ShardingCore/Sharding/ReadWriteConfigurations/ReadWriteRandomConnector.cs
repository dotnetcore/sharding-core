using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShardingCore.Helpers;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/18 20:58:42
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ReadWriteRandomConnector
    {
        private readonly string[] _connectionStrings;
        private readonly int _length;
        public ReadWriteRandomConnector(string dataSourceName,IEnumerable<string> connectionStrings)
        {
            DataSourceName = dataSourceName;
            _connectionStrings = connectionStrings.ToArray();
            _length = _connectionStrings.Length;
        }
        public string GetConnectionString()
        {
            if (_length == 1)
                return _connectionStrings[0];
            var next = RandomHelper.Next(0, _length);
            return _connectionStrings[next];

        }

        public string DataSourceName { get; }
    }
}
