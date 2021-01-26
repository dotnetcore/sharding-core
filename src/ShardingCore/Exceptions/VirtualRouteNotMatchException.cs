using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 15:38:02
* @Email: 326308290@qq.com
*/
    public class VirtualRouteNotMatchException:Exception
    {
        public VirtualRouteNotMatchException()
        {
        }

        protected VirtualRouteNotMatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public VirtualRouteNotMatchException(string message) : base(message)
        {
        }

        public VirtualRouteNotMatchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}