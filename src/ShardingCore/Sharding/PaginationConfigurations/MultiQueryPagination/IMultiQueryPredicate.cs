using System;
using System.Collections.Generic;
using System.Text;

namespace ShardingCore.Sharding.PaginationConfigurations.MultiQueryPagination
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/15 17:07:30
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 多次查询条件
    /// </summary>
    public interface IMultiQueryPredicate
    {
        /// <summary>
        /// 是否继续执行多次查询
        /// </summary>
        /// <param name="total">总total</param>
        /// <param name="stillNeedSkip">还需跳过数目</param>
        /// <param name="realContexts">执行的sql具体条数(路由条数)</param>
        /// <param name="alreadyExecuteTimes">已经执行了多少次了</param>
        /// <returns></returns>
        public bool Continue(long total, int stillNeedSkip, int realContexts,int alreadyExecuteTimes);
    }
}
