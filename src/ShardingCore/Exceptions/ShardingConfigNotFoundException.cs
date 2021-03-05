using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ShardingCore.Exceptions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/5 8:11:30
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingConfigNotFoundException:Exception
    {
        public ShardingConfigNotFoundException()
        {
        }

        protected ShardingConfigNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ShardingConfigNotFoundException(string message) : base(message)
        {
        }

        public ShardingConfigNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
