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
        IQueryable GetRewriteQueryable();
    }
}