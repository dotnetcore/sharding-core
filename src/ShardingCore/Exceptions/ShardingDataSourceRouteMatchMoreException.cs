using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 06 February 2021 09:08:49
* @Email: 326308290@qq.com
*/
    public class ShardingDataSourceRouteMatchMoreException:Exception
    {
        public ShardingDataSourceRouteMatchMoreException()
        {
        }

        protected ShardingDataSourceRouteMatchMoreException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ShardingDataSourceRouteMatchMoreException(string message) : base(message)
        {
        }

        public ShardingDataSourceRouteMatchMoreException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}