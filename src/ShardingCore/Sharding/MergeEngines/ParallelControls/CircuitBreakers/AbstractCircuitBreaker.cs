using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;

namespace ShardingCore.Sharding.MergeEngines.ParallelControls.CircuitBreakers
{
    internal  abstract class AbstractCircuitBreaker: ICircuitBreaker
    {
        private readonly ISeqQueryProvider _seqQueryProvider;
        private const int TRIP = 1;
        private const int UNTRIP = 0;
        private int _trip = UNTRIP;
        private Action _afterTrip;

        protected AbstractCircuitBreaker(ISeqQueryProvider seqQueryProvider)
        {
            _seqQueryProvider = seqQueryProvider;
        }

        protected ISeqQueryProvider GetSeqQueryProvider()
        {
            return _seqQueryProvider;
        }
        public bool IsTrip<TResult>(IEnumerable<TResult> results)
        {

            if (_trip == TRIP)
                return true;
            if (_seqQueryProvider.IsSeqQuery())
            {
                if (_seqQueryProvider.CanTrip())
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
