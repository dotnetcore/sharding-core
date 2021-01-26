using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.DbContexts.ShardingDbContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 16 December 2020 16:15:43
* @Email: 326308290@qq.com
*/
    public class ShardingDbContextOptions
    {

        public ShardingDbContextOptions(DbContextOptions dbContextOptions, string tail, List<VirtualTableDbContextConfig> virtualTableDbContextConfigs, bool removeShardingEntity=false)
        {
            DbContextOptions = dbContextOptions;
            Tail = tail;
            VirtualTableDbContextConfigs = virtualTableDbContextConfigs;
            RemoveShardingEntity = removeShardingEntity;
        }

        public DbContextOptions  DbContextOptions { get; }
        public string Tail { get; }
        public List<VirtualTableDbContextConfig> VirtualTableDbContextConfigs { get; }
        public bool RemoveShardingEntity { get;}
    }
}