using System;
using System.Collections.Concurrent;
using System.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Internals;
using ShardingCore.Sharding.ShardingComparision.Abstractions;

namespace ShardingCore.Sharding.ShardingComparision
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Sunday, 31 October 2021 15:39:46
    * @Email: 326308290@qq.com
    */
    public class CSharpLanguageShardingComparer : IShardingComparer
    {
        private readonly ConcurrentDictionary<Type, object> _comparers = new ();
        public virtual int Compare(IComparable x, IComparable y, bool asc)
        {
            if (x is Guid xg && y is Guid yg)
            {
                return new SqlGuid(xg).SafeCompareToWith(new SqlGuid(yg), asc);
            }
            return x.SafeCompareToWith(y, asc);
        }

        public object CreateComparer(Type comparerType)
        {
            var comparer = _comparers.GetOrAdd(comparerType,
                key => Activator.CreateInstance(typeof(InMemoryShardingComparer<>).GetGenericType0(comparerType),
                    this));
            return comparer;
        }
    }
}