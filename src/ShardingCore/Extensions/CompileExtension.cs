using System;
using ShardingCore.Sharding.Parsers.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

/*
* @Author: xjm
* @Description:
* @Date: DATE TIME
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Extensions
{
    public static class CompileExtension
    {
        /// <summary>
        /// 是否存在自定义查询
        /// </summary>
        /// <param name="prepareParseResult"></param>
        /// <returns></returns>
        public static bool HasCustomerQuery(this IPrepareParseResult prepareParseResult)
        {
            //compileParameter.ReadOnly().HasValue || compileParameter.GetAsRoute() != null;
            return prepareParseResult.GetAsRoute() != null;
        }
    }
}