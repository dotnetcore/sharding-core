using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core;

namespace ShardingCore.Sharding.MergeContexts
{
    public sealed class OptimizeResult: IOptimizeResult
    {
        private readonly int _maxQueryConnectionsLimit;
        private readonly ConnectionModeEnum _connectionMode;
        private readonly bool _isSequenceQuery;
        private readonly bool _sameWithTailComparer;
        private readonly IComparer<string> _shardingTailComparer;
        private readonly bool _canTrip;

        public OptimizeResult(int maxQueryConnectionsLimit, ConnectionModeEnum connectionMode,bool isSequenceQuery,bool sameWithTailComparer,IComparer<string> shardingTailComparer,bool canTrip)
        {
            _maxQueryConnectionsLimit = maxQueryConnectionsLimit;
            _connectionMode = connectionMode;
            _isSequenceQuery = isSequenceQuery;
            _sameWithTailComparer = sameWithTailComparer;
            _shardingTailComparer = shardingTailComparer;
            _canTrip = canTrip;
        }
        public int GetMaxQueryConnectionsLimit()
        {
            return _maxQueryConnectionsLimit;
        }

        public ConnectionModeEnum GetConnectionMode()
        {
            return _connectionMode;
        }

        public bool IsSequenceQuery()
        {
            return _isSequenceQuery;
        }

        public bool SameWithTailComparer()
        {
            return _sameWithTailComparer;
        }

        public IComparer<string> ShardingTailComparer()
        {
            return _shardingTailComparer;
        }
        public bool CanTrip()
        {
            return _canTrip;
        }

    }
}
