using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.DbContexts.ShardingTableDbContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 18 February 2021 15:22:46
* @Email: 326308290@qq.com
*/
    public class ShardingTableDbContextOptions
    {
        
        public ShardingTableDbContextOptions(DbContextOptions dbContextOptions, string tail, List<VirtualTableDbContextConfig> virtualTableDbContextConfigs, bool removeShardingEntity=false)
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