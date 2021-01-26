using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 21:36:24
* @Email: 326308290@qq.com
*/
    public class QueryableRouteNotMatchException:Exception
    {
        public QueryableRouteNotMatchException()
        {
        }

        protected QueryableRouteNotMatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public QueryableRouteNotMatchException(string? message) : base(message)
        {
        }

        public QueryableRouteNotMatchException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}