using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/6 16:54:23
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingReadWriteAccessor<TShardingDbContext>:IShardingReadWriteAccessor where TShardingDbContext:DbContext,IShardingDbContext
    {
        private static AsyncLocal<ShardingReadWriteContext> _shardingReadWriteContext = new AsyncLocal<ShardingReadWriteContext>();

        public Type ShardingDbContextType => typeof(TShardingDbContext);

        /// <summary>
        /// 
        /// </summary>
        public ShardingReadWriteContext ShardingReadWriteContext
        {
            get => _shardingReadWriteContext.Value;
            set => _shardingReadWriteContext.Value = value;
        }
    }
}
