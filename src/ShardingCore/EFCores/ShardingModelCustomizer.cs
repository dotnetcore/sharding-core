using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Extensions;
using ShardingCore.Logger;
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
        private static readonly ILogger<ShardingModelCustomizer<TShardingDbContext>> _logger =
            InternalLoggerFactory.CreateLogger<ShardingModelCustomizer<TShardingDbContext>>();
        private Type _shardingDbContextType => typeof(TShardingDbContext);
        private readonly IEntityMetadataManager<TShardingDbContext> _entityMetadataManager;

        public ShardingModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
        {
            _entityMetadataManager = ShardingContainer.GetService<IEntityMetadataManager<TShardingDbContext>>();
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);
            if (context is IShardingTableDbContext shardingTableDbContext&& shardingTableDbContext.RouteTail !=null&& shardingTableDbContext.RouteTail.IsShardingTableQuery())
            {
                var isMultiEntityQuery = shardingTableDbContext.RouteTail.IsMultiEntityQuery();
                if (!isMultiEntityQuery)
                {
                    var singleQueryRouteTail = (ISingleQueryRouteTail) shardingTableDbContext.RouteTail;
                    var tail = singleQueryRouteTail.GetTail();

                    //设置分表
                    var mutableEntityTypes = modelBuilder.Model.GetEntityTypes().Where(o => _entityMetadataManager.IsShardingTable(o.ClrType)).ToArray();
                    foreach (var entityType in mutableEntityTypes)
                    {
                        MappingToTable(entityType.ClrType, modelBuilder, tail);
                    }
                }
                else
                {
                    var multiQueryRouteTail = (IMultiQueryRouteTail) shardingTableDbContext.RouteTail;
                    var entityTypes = multiQueryRouteTail.GetEntityTypes();
                    var mutableEntityTypes = modelBuilder.Model.GetEntityTypes().Where(o => _entityMetadataManager.IsShardingTable(o.ClrType) && entityTypes.Contains(o.ClrType)).ToArray();
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
            var entityMetadata = _entityMetadataManager.TryGet(clrType);
            var shardingEntity = entityMetadata.EntityType;
            var tableSeparator = entityMetadata.TableSeparator;
            var entity = modelBuilder.Entity(shardingEntity);
            var tableName = entityMetadata.VirtualTableName;
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException($"{shardingEntity}: not found original table name。");
            _logger.LogDebug($"mapping table :[tableName]-->[{tableName}{tableSeparator}{tail}]");
            entity.ToTable($"{tableName}{tableSeparator}{tail}");
        }
    }
}