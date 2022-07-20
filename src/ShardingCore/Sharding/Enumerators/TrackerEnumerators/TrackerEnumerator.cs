using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.QueryTrackers;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.Enumerators.TrackerEnumerators
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/23 23:05:41
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class TrackerEnumerator<T>: IEnumerator<T>
    {
        private readonly IShardingDbContext _shardingDbContext;
        private readonly IEnumerator<T> _enumerator;
        private readonly IQueryTracker _queryTrack;

        public TrackerEnumerator(IShardingDbContext shardingDbContext,IEnumerator<T> enumerator)
        {
            var shardingRuntimeContext = ((DbContext)shardingDbContext).GetShardingRuntimeContext();
            _shardingDbContext = shardingDbContext;
            _enumerator = enumerator;
            _queryTrack = shardingRuntimeContext.GetQueryTracker();
        }
        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
             _enumerator.Reset();
        }

        public T Current => GetCurrent();

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _enumerator.Dispose();
        }
        private T GetCurrent()
        {
            var current = _enumerator.Current;
            if (current != null)
            {
                var attachedEntity = _queryTrack.Track(current, _shardingDbContext);
                if (attachedEntity != null)
                {
                    return (T)attachedEntity;
                }
            }
            return current;
        }
    }
}
