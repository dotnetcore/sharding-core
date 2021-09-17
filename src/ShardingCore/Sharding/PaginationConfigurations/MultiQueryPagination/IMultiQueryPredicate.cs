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
    public interface IMultiQueryPredicate
    {
        public bool Continue(long total, int currentSkip, int tables);
    }
}
