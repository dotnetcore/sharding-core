using System;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 31 October 2021 15:39:46
* @Email: 326308290@qq.com
*/
    public class CSharpLanguageShardingComparer<TShardingDbContext>:IShardingComparer<TShardingDbContext> where TShardingDbContext:DbContext,IShardingDbContext
    {
        public int Compare(IComparable x, IComparable y, bool asc)
        {
            return x.SafeCompareToWith(y, asc);
        }
    }
}