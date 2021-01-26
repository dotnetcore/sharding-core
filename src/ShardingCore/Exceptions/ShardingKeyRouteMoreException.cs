using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 19 December 2020 16:25:21
* @Email: 326308290@qq.com
*/
    public class ShardingKeyRouteMoreException:Exception
    {
        public ShardingKeyRouteMoreException()
        {
        }

        protected ShardingKeyRouteMoreException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ShardingKeyRouteMoreException(string? message) : base(message)
        {
        }

        public ShardingKeyRouteMoreException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}