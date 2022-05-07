using System;
using System.Collections.Generic;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Executors.CircuitBreakers
{
    internal  abstract class AbstractCircuitBreaker: ICircuitBreaker
    {
        private readonly StreamMergeContext _streamMergeContext;
        private const int TRIP = 1;
        private const int UNTRIP = 0;
        private int _trip = UNTRIP;
        private Action _afterTrip;

        protected AbstractCircuitBreaker(StreamMergeContext streamMergeContext)
        {
            _streamMergeContext = streamMergeContext;
        }

        protected StreamMergeContext GetStreamMergeContext()
        {
            return _streamMergeContext;
        }
        public bool IsTrip<TResult>(IEnumerable<TResult> results)
        {

            if (_trip == TRIP)
                return true;
            if (_streamMergeContext.IsSeqQuery())
            {
                if (_streamMergeContext.CanTrip())
                {
                    if (SeqConditionalTrip(results))
                    {
                        Trip();
                        return true;
                    }
                }
            }
            else
            {
                if (RandomConditionalTrip(results))
                {
                    Trip();
                    return true;
                }
            }

            return false;
        }

        protected abstract bool SeqConditionalTrip<TResult>(IEnumerable<TResult> results);
        protected abstract bool RandomConditionalTrip<TResult>(IEnumerable<TResult> results);

        public void Trip()
        {
            _trip = TRIP;
            _afterTrip?.Invoke();
        }

        public void Register(Action afterTrip)
        {
            _afterTrip = afterTrip;
        }
    }
}
