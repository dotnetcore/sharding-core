using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
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
    public class ShardingModelCustomizer<TShardingDbContext> : ModelCustomizer where TShardingDbContext : DbContext, IShardingDbContext
    {
        private Type _shardingDbContextType => typeof(TShardingDbContext);

        public ShardingModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
        {
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);
            if (context is IShardingTableDbContext shardingTableDbContext)
            {
                var isMultiEntityQuery = shardingTableDbContext.RouteTail.IsMultiEntityQuery();
                if (!isMultiEntityQuery)
                {
                    var singleQueryRouteTail = (ISingleQueryRouteTail) shardingTableDbContext.RouteTail;
                    var tail = singleQueryRouteTail.GetTail();
                    var virtualTableManager = ShardingContainer.GetService<IVirtualTableManager<TShardingDbContext>>();
                    var typeMap = virtualTableManager.GetAllVirtualTables().Where(o => o.GetTableAllTails().Contains(tail)).Select(o => o.EntityType).ToHashSet();

                    //设置分表
                    var mutableEntityTypes = modelBuilder.Model.GetEntityTypes().Where(o => o.ClrType.IsShardingTable() && typeMap.Contains(o.ClrType));
                    foreach (var entityType in mutableEntityTypes)
                    {
                        MappingToTable(entityType.ClrType, modelBuilder, tail);
                    }
                }
                else
                {
                    var multiQueryRouteTail = (IMultiQueryRouteTail) shardingTableDbContext.RouteTail;
                    var entityTypes = multiQueryRouteTail.GetEntityTypes();
                    var mutableEntityTypes = modelBuilder.Model.GetEntityTypes().Where(o => o.ClrType.IsShardingTable() && entityTypes.Contains(o.ClrType)).ToArray();
                    foreach (var entityType in mutableEntityTypes)
                    {
                        var queryTail = multiQueryRouteTail.GetEntityTail(entityType.ClrType);
                        if (queryTail != null)
                        {
                            MappingToTable(entityType.ClrType, modelBuilder, queryTail);
                        }
                    }
                }
            }
        }

        private void MappingToTable(Type clrType, ModelBuilder modelBuilder, string tail)
        {
            var shardingEntityConfig = ShardingUtil.Parse(clrType);
            var shardingEntity = shardingEntityConfig.EntityType;
            var tailPrefix = shardingEntityConfig.TailPrefix;
            var entity = modelBuilder.Entity(shardingEntity);
            var tableName = shardingEntityConfig.VirtualTableName;
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException($"{shardingEntity}: not found original table name。");
#if DEBUG
            Console.WriteLine($"mapping table :[tableName]-->[{tableName}{tailPrefix}{tail}]");
#endif
            entity.ToTable($"{tableName}{tailPrefix}{tail}");
        }
    }
}