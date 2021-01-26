using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 15:01:16
* @Email: 326308290@qq.com
*/
    public class VirtualTableNotFoundException:Exception
    {
        public VirtualTableNotFoundException()
        {
        }

        protected VirtualTableNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public VirtualTableNotFoundException(string? message) : base(message)
        {
        }

        public VirtualTableNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}