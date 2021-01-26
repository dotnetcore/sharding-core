using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 13 January 2021 11:09:33
* @Email: 326308290@qq.com
*/
    public class ShardingKeyGetValueException:Exception
    {
        public ShardingKeyGetValueException()
        {
        }

        protected ShardingKeyGetValueException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ShardingKeyGetValueException(string message) : base(message)
        {
        }

        public ShardingKeyGetValueException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}