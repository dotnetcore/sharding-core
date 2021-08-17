using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/17 21:03:03
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingTempInternalDbSet<TEntity> :
        IQueryable<TEntity>,
        IEnumerable<TEntity>,
        IEnumerable,
        IQueryable where TEntity:class
    {
        private IQueryable<TEntity> _queryable;
        public ShardingTempInternalDbSet(EnumerableQuery<TEntity> queryable)
        {
            _queryable = queryable.AsQueryable();
        }
        public IEnumerator<TEntity> GetEnumerator()
        {
            return _queryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type ElementType => _queryable.ElementType;
        public Expression Expression => _queryable.Expression;
        public IQueryProvider Provider => _queryable.Provider;
    }
}
