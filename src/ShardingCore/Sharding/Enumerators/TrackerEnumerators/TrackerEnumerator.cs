using System.Collections;
using System.Collections.Generic;
using ShardingCore.Extensions;

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
        private readonly StreamMergeContext<T> _streamMergeContext;
        private readonly IEnumerator<T> _enumerator;

        public TrackerEnumerator(StreamMergeContext<T> streamMergeContext,IEnumerator<T> enumerator)
        {
            _streamMergeContext = streamMergeContext;
            _enumerator = enumerator;
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
                var c = (object)current;
                var genericDbContext = _streamMergeContext.GetShardingDbContext().CreateGenericDbContext(c);
                var attachedEntity = genericDbContext.GetAttachedEntity(c);
                if (attachedEntity == null)
                {
                    genericDbContext.Attach(current);
                }
                else
                {
                    return (T)attachedEntity;
                }
            }
            return current;
        }
    }
}
