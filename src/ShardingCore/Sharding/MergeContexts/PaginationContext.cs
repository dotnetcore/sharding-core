using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Exceptions;

namespace ShardingCore.Sharding.MergeContexts
{
    public sealed class PaginationContext
    {
        public int? Skip { get; private set; }
        public int? Take { get; private set; }

        public bool HasSkip()
        {
            return Skip.HasValue;
        }

        public bool HasTake()
        {
            return Take.HasValue;
        }

        public void AddSkip(int skip)
        {
            if (Skip.HasValue)
            {
                throw new ShardingCoreNotSupportException("multi skip");
            }

            Skip = skip;
        }

        public void AddTake(int take)
        {
            if (Take.HasValue)
            {
                throw new ShardingCoreNotSupportException("multi take");
            }

            Take = take;
        }

        /// <summary>
        /// 替换为固定的take一般用于first 1 single 2 last 1
        /// </summary>
        /// <param name="take"></param>
        public void ReplaceToFixedTake(int take)
        {
            Take = take;
        }

        public override string ToString()
        {
            return $"{nameof(Skip)}: {Skip},  {nameof(Take)}: {Take}";
        }
    }
}