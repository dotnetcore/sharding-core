using System;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

/*
* @Author: xjm
* @Description:
* @Date: DATE TIME
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Extensions
{
    public static class CompileParameterExtension
    {
        /// <summary>
        /// 是否存在自定义查询
        /// </summary>
        /// <param name="compileParameter"></param>
        /// <returns></returns>
        public static bool HasCustomerQuery(this ICompileParameter compileParameter)
        {
            return compileParameter.ReadOnly().HasValue || compileParameter.GetAsRoute() != null;
        }
    }
}