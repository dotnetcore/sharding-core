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
        private readonly ConcurrentDictionary<Type, IShardingReadWriteAccessor> _shardingReadWriteAccessors;


        public ShardingReadWriteManager(IEnumerable<IShardingReadWriteAccessor> shardingReadWriteAccessors)
        {

            _shardingReadWriteAccessors = new ConcurrentDictionary<Type, IShardingReadWriteAccessor>(shardingReadWriteAccessors.ToDictionary(o => o.ShardingDbContextType, o => o));
        }
        public ShardingReadWriteContext GetCurrent<TShardingDbContext>() where TShardingDbContext : DbContext, IShardingDbContext
        {
            return GetCurrent(typeof(TShardingDbContext));
        }

        public ShardingReadWriteContext GetCurrent(Type shardingDbContextType)
        {
            if (!shardingDbContextType.IsShardingDbContext())
                throw new ShardingCoreInvalidOperationException(shardingDbContextType.FullName);

            if (_shardingReadWriteAccessors.TryGetValue(shardingDbContextType, out var accessor))
                return accessor.ShardingReadWriteContext;
            throw new ShardingCoreInvalidOperationException(shardingDbContextType.FullName);
        }

        public ShardingReadWriteScope<TShardingDbContext> CreateScope<TShardingDbContext>() where TShardingDbContext : DbContext, IShardingDbContext
        {
            var shardingPageScope = new ShardingReadWriteScope<TShardingDbContext>(_shardingReadWriteAccessors.Select(o => o.Value));
            shardingPageScope.ShardingReadWriteAccessor.ShardingReadWriteContext = ShardingReadWriteContext.Create();
            return shardingPageScope;
        }
    }
}
