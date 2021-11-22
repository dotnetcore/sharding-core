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
    public class ShardingKeyGetValueException:ShardingCoreException
    {
        public ShardingKeyGetValueException(string message) : base(message)
        {
        }
    }
}