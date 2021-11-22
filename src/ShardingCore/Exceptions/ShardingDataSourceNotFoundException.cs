using System;
using System.Runtime.Serialization;

namespace ShardingCore.Exceptions
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 21 December 2020 09:32:54
* @Email: 326308290@qq.com
*/
    public class ShardingDataSourceNotFoundException:ShardingCoreException
    {

        public ShardingDataSourceNotFoundException(string message) : base(message)
        {
        }
    }
}