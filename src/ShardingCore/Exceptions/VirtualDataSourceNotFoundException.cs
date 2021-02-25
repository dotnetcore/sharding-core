using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 06 February 2021 15:41:19
* @Email: 326308290@qq.com
*/
    public class VirtualDataSourceNotFoundException:Exception
    {
        public VirtualDataSourceNotFoundException()
        {
        }

        protected VirtualDataSourceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public VirtualDataSourceNotFoundException(string message) : base(message)
        {
        }

        public VirtualDataSourceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}