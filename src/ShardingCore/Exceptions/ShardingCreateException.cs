using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 13 January 2021 10:20:47
* @Email: 326308290@qq.com
*/
    public class ShardingCreateException:ShardingCoreException
    {

        public ShardingCreateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}