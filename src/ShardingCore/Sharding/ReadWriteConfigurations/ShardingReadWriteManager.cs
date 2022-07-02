using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/6 21:02:56
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingReadWriteManager:IShardingReadWriteManager
    {
        private readonly IShardingReadWriteAccessor _shardingReadWriteAccessor;


        public ShardingReadWriteManager(IShardingReadWriteAccessor shardingReadWriteAccessor)
        {
            _shardingReadWriteAccessor = shardingReadWriteAccessor;
        }

        public ShardingReadWriteContext GetCurrent()
        {
                return _shardingReadWriteAccessor.ShardingReadWriteContext;
        }

        public ShardingReadWriteScope CreateScope()
        {
            var shardingPageScope = new ShardingReadWriteScope(_shardingReadWriteAccessor);
            shardingPageScope.ShardingReadWriteAccessor.ShardingReadWriteContext = ShardingReadWriteContext.Create();
            return shardingPageScope;
        }
    }
}
