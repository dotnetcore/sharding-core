using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.DbContexts.VirtualDbContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 01 January 2021 20:28:53
* @Email: 326308290@qq.com
*/
    public class ShardingBatchInsertEntry<T> where T:class
    {
        public ShardingBatchInsertEntry(Dictionary<DbContext, List<T>> batchGroups)
        {
            BatchGroups = batchGroups;
        }

        public Dictionary<DbContext,List<T>> BatchGroups { get; }

    }
}