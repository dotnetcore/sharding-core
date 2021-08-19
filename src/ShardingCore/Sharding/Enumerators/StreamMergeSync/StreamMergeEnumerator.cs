using System;
using System.Collections;
using System.Collections.Generic;

namespace ShardingCore.Sharding.Enumerators.StreamMergeSync
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 14 August 2021 21:25:50
* @Email: 326308290@qq.com
*/
    public class StreamMergeEnumerator<T>:IStreamMergeEnumerator<T>
    {
        private readonly IEnumerator<T> _source;
        private bool skip;

        public StreamMergeEnumerator(IEnumerator<T> source)
        {
            _source = source;
            skip = true;
        }

        public bool MoveNext()
        {
            if (skip)
            {
                skip = false;
                return null != _source.Current;
            }
            return  _source.MoveNext();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        object IEnumerator.Current => Current;

        public T Current => skip?default:_source.Current;
        public bool SkipFirst()
        {
            if (skip)
            {
                skip = false;
                return true;
            }
            return false;
        }

        public bool HasElement()
        {
            return null != _source.Current;
        }

        public T ReallyCurrent => _source.Current;
        public void Dispose()
        {
            _source?.Dispose();
        }
    }
}