using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Extensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/15 16:12:27
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 
    /// </summary>
    public static class ShardingExtension
    {
        private static readonly string ShardingTableDbContextFormat = $"sharding_{Guid.NewGuid():n}_";
        // /// <summary>
        // /// 获取分表的tail
        // /// </summary>
        // /// <param name="dbContext"></param>
        // /// <returns></returns>
        // public static string GetShardingTableDbContextTail(this IShardingTableDbContext dbContext)
        // {
        //     return dbContext.RouteTail?.Replace(ShardingTableDbContextFormat, string.Empty)??string.Empty;
        //
        // }
        // /// <summary>
        // /// 设置分表的tail
        // /// </summary>
        // /// <param name="dbContext"></param>
        // /// <param name="tail"></param>
        // public static void SetShardingTableDbContextTail(this IShardingTableDbContext dbContext, string tail)
        // {
        //     if (!string.IsNullOrWhiteSpace(dbContext.ModelChangeKey))
        //         throw new ShardingCoreException($"repeat set ModelChangeKey in {dbContext.GetType().FullName}");
        //     dbContext.ModelChangeKey = tail.FormatRouteTail();
        // }

        public static string FormatRouteTail2ModelCacheKey(this string originalTail)
        {
            return $"{ShardingTableDbContextFormat}{originalTail}";
            ;
        }
    }
}
