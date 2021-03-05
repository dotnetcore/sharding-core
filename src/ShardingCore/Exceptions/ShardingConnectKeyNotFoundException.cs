using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ShardingCore.Exceptions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/5 13:25:51
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingConnectKeyNotFoundException: Exception
    {
        public ShardingConnectKeyNotFoundException()
        {
        }

        protected ShardingConnectKeyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ShardingConnectKeyNotFoundException(string message) : base(message)
        {
        }

        public ShardingConnectKeyNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
