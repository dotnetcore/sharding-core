using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.Parsers.Abstractions
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/1 16:02:57
    /// Email: 326308290@qq.com
    public interface IPrepareParser
    {
        IPrepareParseResult Parse(IShardingDbContext shardingDbContext, Expression query);
    }
}
