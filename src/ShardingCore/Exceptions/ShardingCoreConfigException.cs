using System;
using System.Diagnostics.CodeAnalysis;

namespace ShardingCore.Exceptions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 04 December 2021 11:26:17
    * @Email: 326308290@qq.com
    */
    [ExcludeFromCodeCoverage]
    public class ShardingCoreConfigException : ShardingCoreException
    {
        public ShardingCoreConfigException(string message) : base(message)
        {
        }

        public ShardingCoreConfigException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
