using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.DbContexts.ShardingDbContexts;

namespace ShardingCore.EFCores
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 16 December 2020 16:13:05
* @Email: 326308290@qq.com
*/
    public class ShardingModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context)
        {
            if (context is ShardingDbContext shardingDbContext)
            {
                //当出现尾巴不一样,本次映射的数据库实体数目不一样就需要重建ef model
                var tail = shardingDbContext.Tail;
                var removeSharding = shardingDbContext.RemoveRemoveShardingEntity;
                var allEntities = string.Join(",",shardingDbContext.VirtualTableConfigs.Select(o=>o.ShardingEntityType.FullName).OrderBy(o=>o).ToList());

                return $"{context.GetType()}_{allEntities}_{tail}_{removeSharding}";
            }
            else
            {
                return context.GetType();
            }
        }
    }
}