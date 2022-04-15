using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace ShardingCore.Core
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/4/15 11:40:58
    /// Email: 326308290@qq.com
    public interface IShardingModelCacheOption
    {
#if !EFCORE2
        CacheItemPriority GetModelCachePriority();
        int GetModelCacheEntrySize();
#endif
        int GetModelCacheLockObjectSeconds();
    }
}
