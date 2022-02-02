using System;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

/*
* @Author: xjm
* @Description:
* @Date: DATE TIME
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Sharding.ShardingExecutors
{
    internal class CustomerReadWriteScope:IDisposable
    {
        private readonly IShardingDbContext _shardingDbContext;
        private readonly bool _readOnly;

        public CustomerReadWriteScope(IShardingDbContext shardingDbContext,bool readOnly)
        {
            _shardingDbContext = shardingDbContext;
            _readOnly = readOnly;
            if (_readOnly)
            {
                _shardingDbContext.ReadWriteSeparationReadOnly();
            }
            else
            {
                _shardingDbContext.ReadWriteSeparationWriteOnly();
            }
        }

        public void Dispose()
        {
            if (_readOnly)
            {
                _shardingDbContext.ReadWriteSeparationWriteOnly();
            }
            else
            {
                _shardingDbContext.ReadWriteSeparationReadOnly();
            }
        }
    }
}