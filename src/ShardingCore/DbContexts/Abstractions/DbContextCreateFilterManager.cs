using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShardingCore.DbContexts.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/13 8:19:26
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DbContextCreateFilterManager: IDbContextCreateFilterManager
    {
        private readonly List<IDbContextCreateFilter> _filters = new List<IDbContextCreateFilter>();
        public void RegisterFilter(IDbContextCreateFilter filter)
        {
            if (null == filter)
                throw new ArgumentNullException(nameof(filter));
            if(!_filters.Contains(filter))
                _filters.Add(filter);
        }

        public List<IDbContextCreateFilter> GetFilters()
        {
            return _filters;
        }
    }
}
