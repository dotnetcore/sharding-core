using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.DbContexts.ShardingDbContexts;

namespace ShardingCore.DbContexts.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 07 February 2021 15:24:41
* @Email: 326308290@qq.com
*/
    public abstract class AbstractionShardingTableDbContext:DbContext,IShardingTableDbContext
    {
        public AbstractionShardingTableDbContext(DbContextOptions options):base(options)
        {
            
        }

        private IShardingCruder _shardingCruder;
        public IShardingCruder ShardingCruder
        {
            get
            {
                if (_shardingCruder == null)
                {
                    _shardingCruder = new ShardingCruder(this);
                }

                return _shardingCruder;
            }
        }

        public string ModelChangeKey { get; set; }
    }
}