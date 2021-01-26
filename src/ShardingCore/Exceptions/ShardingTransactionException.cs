using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 10:40:34
* @Email: 326308290@qq.com
*/
    public class ShardingTransactionException:Exception
    {
        public ShardingTransactionException()
        {
        }

        protected ShardingTransactionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ShardingTransactionException(string? message) : base(message)
        {
        }

        public ShardingTransactionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}