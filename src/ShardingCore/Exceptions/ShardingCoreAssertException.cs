using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 25 August 2021 19:20:14
* @Email: 326308290@qq.com
*/
    public class ShardingCoreAssertException:ShardingCoreException
    {
        
        public ShardingCoreAssertException()
        {
        }

        protected ShardingCoreAssertException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ShardingCoreAssertException(string message) : base(message)
        {
        }

        public ShardingCoreAssertException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}