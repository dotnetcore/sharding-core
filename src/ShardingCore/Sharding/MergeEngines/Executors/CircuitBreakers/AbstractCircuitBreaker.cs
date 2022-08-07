using System;
using System.Collections.Generic;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Executors.CircuitBreakers
{
    internal abstract class AbstractCircuitBreaker : ICircuitBreaker
    {
        private readonly StreamMergeContext _streamMergeContext;
        private const int TERMINATED = 1;
        private const int UNTERMINATED = 0;
        private int _terminated = UNTERMINATED;
        private Action _afterTerminated;

        protected AbstractCircuitBreaker(StreamMergeContext streamMergeContext)
        {
            _streamMergeContext = streamMergeContext;
        }

        protected StreamMergeContext GetStreamMergeContext()
        {
            return _streamMergeContext;
        }

        public bool Terminated<TResult>(IEnumerable<TResult> results)
        {
            if (_terminated == TERMINATED)
                return true;
            if (_streamMergeContext.IsSeqQuery())
            {
                if (OrderConditionTerminated(results))
                {
                    Terminated0();
                    return true;
                }
            }
            else
            {
                if (RandomConditionTerminated(results))
                {
                    Terminated0();
                    return true;
                }
            }

            return false;
        }

        protected abstract bool OrderConditionTerminated<TResult>(IEnumerable<TResult> results);
        protected abstract bool RandomConditionTerminated<TResult>(IEnumerable<TResult> results);

        public void Terminated0()
        {
            _terminated = TERMINATED;
            _afterTerminated?.Invoke();
        }

        public void Register(Action afterTerminated)
        {
            _afterTerminated = afterTerminated;
        }
    }
}