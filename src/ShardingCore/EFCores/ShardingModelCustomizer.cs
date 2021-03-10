using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Extensions;
using ShardingCore.Utils;

namespace ShardingCore.EFCores
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/8 14:54:51
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingModelCustomizer: ModelCustomizer
    {
        public ShardingModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
        {
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);
            if (context is IShardingTableDbContext)
            {
                var shardingTableAccessor = ShardingContainer.Services.GetService<IShardingTableAccessor>();
            
                if (shardingTableAccessor.Context != null)
                {
                    if (!string.IsNullOrWhiteSpace(shardingTableAccessor.Context.Tail))
                    {
                        var virtualTableManager = ShardingContainer.Services.GetService<IVirtualTableManager>();
                        var connectKey = shardingTableAccessor.Context.ConnectKey;
                        var tail = shardingTableAccessor.Context.Tail;
                        var typeMap = virtualTableManager.GetAllVirtualTables(connectKey).Where(o=>o.GetTaleAllTails().Contains(tail)).Select(o=>o.EntityType).ToHashSet();

                        //设置分表
                        var mutableEntityTypes = modelBuilder.Model.GetEntityTypes().Where(o => o.ClrType.IsShardingTable()&&typeMap.Contains(o.ClrType));
                        foreach (var entityType in mutableEntityTypes)
                        {
                            var shardingEntityConfig = ShardingKeyUtil.Parse(entityType.ClrType);
                            var shardingEntity = shardingEntityConfig.ShardingEntityType;
                            var tailPrefix = shardingEntityConfig.TailPrefix;
                            var entity = modelBuilder.Entity(shardingEntity);
                            var tableName = shardingEntityConfig.ShardingOriginalTable;
                            if (string.IsNullOrWhiteSpace(tableName))
                                throw new ArgumentNullException($"{shardingEntity}: not found original table name。");
#if DEBUG
                            Console.WriteLine($"mapping table :[tableName]-->[{tableName}{tailPrefix}{tail}]");
#endif
                            entity.ToTable($"{tableName}{tailPrefix}{tail}");
                        }
                    }
                }
            }
        }
    }
}
