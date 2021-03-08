using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;

namespace ShardingCore.DbContexts.ShardingDbContexts
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/4 16:11:18
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractShardingTableDbContext : DbContext
    {
        public string Tail { get; }
        public Dictionary<Type, VirtualTableDbContextConfig> VirtualTableConfigs { get; }

        protected AbstractShardingTableDbContext(ShardingDbContextOptions options):base(options.DbContextOptions)
        {
            Tail = options.Tail;
            VirtualTableConfigs = options.VirtualTableDbContextConfigs.ToDictionary(o => o.ShardingEntityType, o => o);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            OnShardingModelCreating(modelBuilder);
            OnModelCreatingAfter(modelBuilder);
        }

        protected abstract void OnShardingModelCreating(ModelBuilder modelBuilder);

        protected virtual void OnModelCreatingAfter(ModelBuilder modelBuilder)
        {
            if (!string.IsNullOrWhiteSpace(Tail))
            {
                if (VirtualTableConfigs.IsNotEmpty())
                {
                    var mutableEntityTypes = modelBuilder.Model.GetEntityTypes().Where(o => VirtualTableConfigs.ContainsKey(o.ClrType));
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
}