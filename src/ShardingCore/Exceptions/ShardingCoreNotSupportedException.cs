using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ShardingCore.Exceptions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/16 14:08:08
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingCoreNotSupportedException:NotSupportedException
    {
        public ShardingCoreNotSupportedException()
        {
        }

        protected ShardingCoreNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ShardingCoreNotSupportedException(string message) : base(message)
        {
        }

        public ShardingCoreNotSupportedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
