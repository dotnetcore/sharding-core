using System;
using System.Linq;

namespace ShardingCore.Sharding.MergeContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 02 March 2022 21:38:18
* @Email: 326308290@qq.com
*/
    public interface IRewriteResult
    {
        /// <summary>
        /// 最原始的表达式
        /// </summary>
        /// <returns></returns>
        IQueryable GetCombineQueryable();
        /// <summary>
        /// 被重写后的表达式
        /// </summary>
        /// <returns></returns>
        IQueryable GetRewriteQueryable();
    }
}