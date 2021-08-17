using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShardingCore.Extensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/17 21:42:37
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public static class ShardingQueryaleExtension
    {
        public static IQueryable<TElement> AsShardingQueryable<TElement>(
            this IEnumerable<TElement> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source is IQueryable<TElement> queryable ? queryable : (IQueryable<TElement>)new EnumerableQuery<TElement>(source);
        }
    }
}
