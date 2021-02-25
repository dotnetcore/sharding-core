using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.DbContexts.ShardingTableDbContexts
{
/*
* @Author: xjm
* @Description: 用于分表使用 分表比较特殊必须使用规定的dbcontext
* @Date: Thursday, 18 February 2021 15:06:46
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 
    /// </summary>
    public abstract class ShardingTableDbContext:DbContext
    {
        public string Tail { get; }
        public Dictionary<Type,VirtualTableDbContextConfig> VirtualTableConfigs { get; }
        public bool RemoveRemoveShardingEntity { get; }
        
        public ShardingTableDbContext(ShardingTableDbContextOptions options)
        {
            Tail = options.Tail;
            VirtualTableConfigs = options.VirtualTableDbContextConfigs.ToDictionary(o=>o.ShardingEntityType,o=>o);
            RemoveRemoveShardingEntity = options.RemoveShardingEntity;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            OnModelCreatingAfter(modelBuilder);
        }

        protected virtual void OnModelCreatingAfter(ModelBuilder modelBuilder)
        {
            if (!string.IsNullOrWhiteSpace(Tail))
            {
                var mutableEntityTypes = modelBuilder.Model.GetEntityTypes().Where(o=>VirtualTableConfigs.ContainsKey(o.ClrType));
                foreach (var entityType in mutableEntityTypes)
                {
                    var virtualTableConfig = VirtualTableConfigs[entityType.ClrType];
                    var shardingEntity = virtualTableConfig.ShardingEntityType;
                    var tailPrefix = virtualTableConfig.TailPrefix;
                    var entity = modelBuilder.Entity(shardingEntity);
                    var tableName = virtualTableConfig.OriginalTableName;
                    if (string.IsNullOrWhiteSpace(tableName))
                        throw new ArgumentNullException($"{shardingEntity}: not found original table name。");
#if DEBUG
                    Console.WriteLine($"映射表:[tableName]-->[{tableName}{tailPrefix}{Tail}]");
#endif
                    entity.ToTable($"{tableName}{tailPrefix}{Tail}");
                }
            }
        }
    }
}