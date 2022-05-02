using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Parsers.Abstractions;
using ShardingCore.Sharding.Parsers.Visitors;

namespace ShardingCore.Sharding.Parsers
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/1 21:37:25
    /// Email: 326308290@qq.com
    public class DefaultPrepareParser:IPrepareParser
    {
        public IPrepareParseResult Parse(IShardingDbContext shardingDbContext, Expression query)
        {
            var shardingQueryPrepareVisitor = new ShardingQueryPrepareVisitor(shardingDbContext);
            var expression = shardingQueryPrepareVisitor.Visit(query);
            var shardingPrepareResult = shardingQueryPrepareVisitor.GetShardingPrepareResult();
            return new PrepareParseResult(shardingDbContext, expression, shardingPrepareResult);
        }
    }
}
