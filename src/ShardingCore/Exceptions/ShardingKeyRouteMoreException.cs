using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 19 December 2020 16:25:21
* @Email: 326308290@qq.com
*/
    public class ShardingKeyRouteMoreException:ShardingCoreException
    {

        public ShardingKeyRouteMoreException(string message) : base(message)
        {
        }
    }
}