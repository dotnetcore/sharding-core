using System;
using System.Linq;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.MergeContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 02 March 2022 21:21:26
* @Email: 326308290@qq.com
*/
    public interface IQueryableRewriteEngine
    {
        IRewriteResult GetRewriteQueryable(IMergeQueryCompilerContext mergeQueryCompilerContext, IParseResult parseResult);
    }
}