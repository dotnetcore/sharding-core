using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.ReadWriteConfigurations.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/6 16:31:32
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IShardingReadWriteManager
    {

        ShardingReadWriteContext GetCurrent();

        ShardingReadWriteScope CreateScope();
    }
}
