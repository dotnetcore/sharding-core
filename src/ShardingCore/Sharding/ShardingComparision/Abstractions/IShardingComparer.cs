using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

/*
* @Author: xjm
* @Description:
* @Date: Sunday, 31 October 2021 15:07:52
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Sharding.ShardingComparision.Abstractions
{
    /// <summary>
    /// 分表内存排序比较器
    /// </summary>
    public interface IShardingComparer
    {
        /// <summary>
        /// 比较 参数
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="asc"></param>
        /// <returns></returns>
        int Compare(IComparable a, IComparable b,bool asc);
        /// <summary>
        /// 创建一个比较器
        /// </summary>
        /// <param name="comparerType"></param>
        /// <returns></returns>
        object CreateComparer(Type comparerType);
    }
    /// <summary>
    /// 泛型比较器用于依赖注入
    /// </summary>
    /// <typeparam name="TShardingDbContext"></typeparam>
    public interface IShardingComparer<TShardingDbContext> : IShardingComparer where TShardingDbContext:DbContext,IShardingDbContext
    {
        
    }
}