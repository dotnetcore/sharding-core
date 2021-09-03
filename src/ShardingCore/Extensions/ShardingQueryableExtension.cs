using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingPage.Abstractions;

namespace ShardingCore.Extensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/3 10:36:51
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public static class ShardingQueryableExtension
    {
        public static async Task<ShardingPagedResult<T>> ToShardingPageAsync<T>(this IQueryable<T> source, int pageIndex, int pageSize)
        {
            //设置每次获取多少页
            var take = pageSize <= 0 ? 1 : pageSize;
            //设置当前页码最小1
            var index = pageIndex <= 0 ? 1 : pageIndex;
            //需要跳过多少页
            var skip = (index - 1) * take;
            var shardingPageManager = ShardingContainer.GetService<IShardingPageManager>();
            using (shardingPageManager.CreateScope())
            {
                //获取每次总记录数
                var count = await source.LongCountAsync();
                if (count <= skip)
                    return new ShardingPagedResult<T>(new List<T>(0), count);
                var data = await source.Skip(skip).Take(take).ToListAsync();
                return new ShardingPagedResult<T>(data, count);
            }
        }
        public static ShardingPagedResult<T> ToShardingPage<T>(this IQueryable<T> source, int pageIndex, int pageSize)
        {
            //设置每次获取多少页
            var take = pageSize <= 0 ? 1 : pageSize;
            //设置当前页码最小1
            var index = pageIndex <= 0 ? 1 : pageIndex;
            //需要跳过多少页
            var skip = (index - 1) * take;

            var shardingPageManager = ShardingContainer.GetService<IShardingPageManager>();
            using (shardingPageManager.CreateScope())
            {
                //获取每次总记录数
                var count = source.Count();
                if (count <= skip)
                    return new ShardingPagedResult<T>(new List<T>(0), count);
                var data = source.Skip(skip).Take(take).ToList();
                return new ShardingPagedResult<T>(data, count);
            }
        }
    }
}
