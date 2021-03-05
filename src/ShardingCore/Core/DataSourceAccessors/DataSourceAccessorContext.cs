using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShardingCore.Core.DataSourceAccessors
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/1 16:11:10
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 数据访问器上下文
    /// </summary>
    public class DataSourceAccessorContext<T>
    {
        private readonly IQueryable<T> _queryable;

        public DataSourceAccessorContext(IQueryable<T> queryable)
        {
            _queryable = queryable;
        }
    }
}
