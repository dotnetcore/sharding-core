using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 21 December 2020 09:32:54
* @Email: 326308290@qq.com
*/
    public class ShardingOwnerNotFoundException:Exception
    {
        public ShardingOwnerNotFoundException()
        {
        }

        protected ShardingOwnerNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ShardingOwnerNotFoundException(string? message) : base(message)
        {
        }

        public ShardingOwnerNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}