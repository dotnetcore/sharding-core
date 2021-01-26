using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 21 December 2020 09:50:57
* @Email: 326308290@qq.com
*/
    public class VirtualRouteNotFoundException:Exception
    {
        public VirtualRouteNotFoundException()
        {
        }

        protected VirtualRouteNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public VirtualRouteNotFoundException(string message) : base(message)
        {
        }

        public VirtualRouteNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}