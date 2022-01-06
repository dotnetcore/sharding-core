using System;

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
}