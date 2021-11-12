using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.ShardingComparision.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 31 October 2021 15:07:52
* @Email: 326308290@qq.com
*/
    public interface IShardingComparer
    {
        int Compare(IComparable a, IComparable b,bool asc);
        object CreateComparer(Type comparerType);
    }

    public interface IShardingComparer<TShardingDbContext> : IShardingComparer where TShardingDbContext:DbContext,IShardingDbContext
    {
        
    }
}