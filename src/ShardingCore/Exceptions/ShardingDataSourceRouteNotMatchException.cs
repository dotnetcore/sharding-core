using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 06 February 2021 09:07:39
* @Email: 326308290@qq.com
*/
    public class ShardingDataSourceRouteNotMatchException:ShardingCoreException
    {
        public ShardingDataSourceRouteNotMatchException(string message) : base(message)
        {
        }
    }
}