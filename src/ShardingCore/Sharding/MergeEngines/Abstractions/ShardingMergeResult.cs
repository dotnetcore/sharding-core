using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.Sharding.MergeEngines.Abstractions
{
    internal class ShardingMergeResult<TResult>
    {
        public DbContext DbContext { get; }
        public TResult MergeResult { get; }

        public ShardingMergeResult(DbContext dbContext,TResult mergeResult)
        {
            DbContext = dbContext;
            MergeResult = mergeResult;
        }
    }
}
