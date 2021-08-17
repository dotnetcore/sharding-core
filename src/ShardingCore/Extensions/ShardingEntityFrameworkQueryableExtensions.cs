using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace ShardingCore.Extensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/17 15:51:32
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal static class ShardingEntityFrameworkQueryableExtensions
    {
        public static Task<TSource> ShardingFirstOrDefaultAsync<TSource>(
            this IQueryable<TSource> source,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(source, cancellationToken);
        }
    }
}
