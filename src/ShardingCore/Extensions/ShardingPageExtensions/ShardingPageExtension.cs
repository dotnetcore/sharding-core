using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ShardingCore.EFCores;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Extensions.ShardingPageExtensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/3 10:36:51
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public static class ShardingPageExtension
    {
        private static IShardingDbContext GetShardingDbContext<T>(IQueryable<T> source)
        {
            
            var entityQueryProvider = source.Provider as EntityQueryProvider??throw new ShardingCoreInvalidOperationException($"cant use sharding page that {nameof(IQueryable)} provider not {nameof(EntityQueryProvider)}");

            var shardingQueryCompiler = entityQueryProvider.GetFieldValue("_queryCompiler") as ShardingQueryCompiler??throw new ShardingCoreInvalidOperationException($"cant use sharding page that {nameof(EntityQueryProvider)} not contains {nameof(ShardingQueryCompiler)} filed named _queryCompiler");
            var dbContextAvailable = shardingQueryCompiler as IShardingDbContextAvailable;
            if (dbContextAvailable == null)
            {
                throw new ShardingCoreInvalidOperationException($"cant use sharding page that {nameof(ShardingQueryCompiler)} not impl  {nameof(IShardingDbContextAvailable)}");
            }

            return dbContextAvailable.GetShardingDbContext();
        }
        /// <summary>
        /// 配置了分页configuration
        /// count+list的分页list会根据count进行优化
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<ShardingPagedResult<T>> ToShardingPageAsync<T>(this IQueryable<T> source, int pageIndex, int pageSize)
        {
            var shardingDbContext = GetShardingDbContext(source);
            var shardingRuntimeContext = ((DbContext)shardingDbContext).GetShardingRuntimeContext();
            
            //设置每次获取多少页
            var take = pageSize <= 0 ? 1 : pageSize;
            //设置当前页码最小1
            var index = pageIndex <= 0 ? 1 : pageIndex;
            //需要跳过多少页
            var skip = (index - 1) * take;
            var shardingPageManager = shardingRuntimeContext.GetShardingPageManager();
            using (shardingPageManager.CreateScope())
            {
                //获取每次总记录数
                var count = await source.CountAsync();
                if (count <= skip)
                    return new ShardingPagedResult<T>(new List<T>(0), count);
                //获取剩余条数
                var remainingCount = count - skip;
                //当剩余条数小于take数就取remainingCount
                var realTake = remainingCount < take ? remainingCount : take;
                var data = await source.Skip(skip).Take(realTake).ToListAsync();
                return new ShardingPagedResult<T>(data, count);
            }
        }
        /// <summary>
        /// 配置了分页configuration
        /// count+list的分页list会根据count进行优化
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ShardingPagedResult<T> ToShardingPage<T>(this IQueryable<T> source, int pageIndex, int pageSize)
        {
            var shardingDbContext = GetShardingDbContext(source);
            var shardingRuntimeContext = ((DbContext)shardingDbContext).GetShardingRuntimeContext();
            //设置每次获取多少页
            var take = pageSize <= 0 ? 1 : pageSize;
            //设置当前页码最小1
            var index = pageIndex <= 0 ? 1 : pageIndex;
            //需要跳过多少页
            var skip = (index - 1) * take;

            var shardingPageManager = shardingRuntimeContext.GetShardingPageManager();
            using (shardingPageManager.CreateScope())
            {
                //获取每次总记录数
                var count = source.Count();
                if (count <= skip)
                    return new ShardingPagedResult<T>(new List<T>(0), count);
                //获取剩余条数
                var remainingCount = count - skip;
                //当剩余条数小于take数就取remainingCount
                var realTake = remainingCount < take ? remainingCount : take;
                var data = source.Skip(skip).Take(realTake).ToList();
                return new ShardingPagedResult<T>(data, count);
            }
        }
    }
}
