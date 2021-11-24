using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;

namespace ShardingCore.Exceptions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/10/3 22:25:59
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    [ExcludeFromCodeCoverage]
    public class ShardingCoreParallelQueryTimeOutException:ShardingCoreException
    {

        public ShardingCoreParallelQueryTimeOutException(string message) : base(message)
        {
        }
    }
}
