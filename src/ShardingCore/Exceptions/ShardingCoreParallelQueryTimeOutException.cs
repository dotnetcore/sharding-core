using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ShardingCore.Exceptions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/10/3 22:25:59
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingCoreParallelQueryTimeOutException:ShardingCoreException
    {
        public ShardingCoreParallelQueryTimeOutException()
        {
        }

        protected ShardingCoreParallelQueryTimeOutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ShardingCoreParallelQueryTimeOutException(string message) : base(message)
        {
        }

        public ShardingCoreParallelQueryTimeOutException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
