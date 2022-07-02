using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/6 20:58:57
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingReadWriteScope:IDisposable
    {
        public IShardingReadWriteAccessor ShardingReadWriteAccessor { get; }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="shardingReadWriteAccessor"></param>
        public ShardingReadWriteScope(IShardingReadWriteAccessor shardingReadWriteAccessor)
        {
            ShardingReadWriteAccessor = shardingReadWriteAccessor;
        }

    /// <summary>
    /// 回收
    /// </summary>
    public void Dispose()
    {
        ShardingReadWriteAccessor.ShardingReadWriteContext = null;
    }
    }
}
