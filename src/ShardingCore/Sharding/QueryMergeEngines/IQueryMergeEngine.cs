using System;
using System.Linq;

namespace ShardingCore.Sharding.QueryMergeEngines
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 01 March 2022 22:14:18
* @Email: 326308290@qq.com
*/
    public interface IQueryMergeEngine
    {
        void ParseAndRewrite(IQueryable queryable);
    }
}