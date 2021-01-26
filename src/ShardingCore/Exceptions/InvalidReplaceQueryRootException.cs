using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 15 January 2021 09:07:34
* @Email: 326308290@qq.com
*/
    public class InvalidReplaceQueryRootException:Exception
    {
        public InvalidReplaceQueryRootException()
        {
        }

        protected InvalidReplaceQueryRootException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidReplaceQueryRootException(string message) : base(message)
        {
        }

        public InvalidReplaceQueryRootException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}