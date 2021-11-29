using System;
using System.Collections.Generic;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails;
using ShardingCore.Extensions;

/*
* @Author: xjm
* @Description:
* @Date: Sunday, 28 November 2021 21:47:38
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Sharding.ShardingDbContextExecutors
{
    public class NoShardingFirstComparer:IComparer<string>
    {
        private readonly string _defaultTail;

        public NoShardingFirstComparer()
        {
            _defaultTail = new SingleQueryRouteTail(string.Empty).GetRouteTailIdentity();
        }

        public int Compare(string? x, string? y)
        {
            if (_defaultTail.Equals(x))
                return -1;
            if (_defaultTail.Equals(y))
                return 1;
            return Comparer<string>.Default.Compare(x, y);
        }
    }
}