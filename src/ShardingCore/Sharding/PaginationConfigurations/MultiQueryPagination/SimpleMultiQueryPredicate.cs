using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ShardingCore.Sharding.PaginationConfigurations.MultiQueryPagination
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/29 17:26:06
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    [ExcludeFromCodeCoverage]
    public class SimpleMultiQueryPredicate : IMultiQueryPredicate
    {
        /// <summary>
        /// 如果需要跳过得条数大于5000并且已经执行次数小于路有数最大5次的情况下继续执行多次查询
        /// </summary>
        /// <param name="total"></param>
        /// <param name="stillNeedSkip"></param>
        /// <param name="realContexts"></param>
        /// <param name="alreadyExecuteTimes"></param>
        /// <returns></returns>
        public bool Continue(long total, int stillNeedSkip, int realContexts, int alreadyExecuteTimes)
        {
            if (stillNeedSkip > 5000)
            {
                if (alreadyExecuteTimes <= 5 && alreadyExecuteTimes <= realContexts)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
