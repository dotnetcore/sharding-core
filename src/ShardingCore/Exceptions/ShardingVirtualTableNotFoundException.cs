using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ShardingCore.Exceptions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/5 13:06:39
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingVirtualTableNotFoundException: ShardingCoreException
    {

        public ShardingVirtualTableNotFoundException(string message) : base(message)
        {
        }
    }
}
