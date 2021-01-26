using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 28 December 2020 22:34:00
* @Email: 326308290@qq.com
*/
    public class CreateSqlVirtualTableNotFoundException:Exception
    {
        public CreateSqlVirtualTableNotFoundException()
        {
        }

        protected CreateSqlVirtualTableNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CreateSqlVirtualTableNotFoundException(string message) : base(message)
        {
        }

        public CreateSqlVirtualTableNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}