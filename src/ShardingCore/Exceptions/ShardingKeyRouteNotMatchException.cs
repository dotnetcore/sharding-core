using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 21 December 2020 08:20:56
* @Email: 326308290@qq.com
*/
    public class ShardingKeyRouteNotMatchException:Exception
    {
        public ShardingKeyRouteNotMatchException()
        {
        }

        protected ShardingKeyRouteNotMatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ShardingKeyRouteNotMatchException(string message) : base(message)
        {
        }

        public ShardingKeyRouteNotMatchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}