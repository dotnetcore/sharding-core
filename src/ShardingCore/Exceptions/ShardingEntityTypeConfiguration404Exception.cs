using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 23 December 2020 09:11:57
* @Email: 326308290@qq.com
*/
    public class ShardingEntityTypeConfiguration404Exception:Exception
    {
        public ShardingEntityTypeConfiguration404Exception()
        {
        }

        protected ShardingEntityTypeConfiguration404Exception(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ShardingEntityTypeConfiguration404Exception(string? message) : base(message)
        {
        }

        public ShardingEntityTypeConfiguration404Exception(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}