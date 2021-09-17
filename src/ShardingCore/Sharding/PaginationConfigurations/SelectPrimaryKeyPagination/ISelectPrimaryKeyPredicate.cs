using System;
using System.Collections.Generic;
using System.Text;

namespace ShardingCore.Sharding.PaginationConfigurations.SelectPrimaryKeyPagination
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/15 22:17:57
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface ISelectPrimaryKeyPredicate
    {
        public bool ShouldUse(long total, int skip, int tables);
    }
}
