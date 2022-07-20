using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;

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
    public class ShardingModelCustomizer : ModelCustomizer
    {

        public ShardingModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
        {
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);
       
            if (context is IShardingTableDbContext shardingTableDbContext&& shardingTableDbContext.RouteTail !=null&& shardingTableDbContext.RouteTail.IsShardingTableQuery())
            {
                var shardingRuntimeContext = context.GetShardingRuntimeContext(); 
                var entityMetadataManager = shardingRuntimeContext.GetEntityMetadataManager();
                var isMultiEntityQuery = shardingTableDbContext.RouteTail.IsMultiEntityQuery();
                if (!isMultiEntityQuery)
                {
                    var singleQueryRouteTail = (ISingleQueryRouteTail) shardingTableDbContext.RouteTail;
                    var tail = singleQueryRouteTail.GetTail();

                    //设置分表
                    var mutableEntityTypes = modelBuilder.Model.GetEntityTypes().Where(o => entityMetadataManager.IsShardingTable(o.ClrType)).ToArray();
                    foreach (var entityType in mutableEntityTypes)
                    {
                        MappingToTable(entityMetadataManager,entityType, modelBuilder, tail);
                    }
                }
                else
                {
                    var multiQueryRouteTail = (IMultiQueryRouteTail) shardingTableDbContext.RouteTail;
                    var entityTypes = multiQueryRouteTail.GetEntityTypes();
                    var mutableEntityTypes = modelBuilder.Model.GetEntityTypes().Where(o => entityMetadataManager.IsShardingTable(o.ClrType) && entityTypes.Contains(o.ClrType)).ToArray();
                    foreach (var entityType in mutableEntityTypes)
                    {
                        var queryTail = multiQueryRouteTail.GetEntityTail(entityType.ClrType);
                        if (queryTail != null)
                        {
                            MappingToTable(entityMetadataManager,entityType, modelBuilder, queryTail);
                        }
                    }
                }
            }
        }

        private void MappingToTable(IEntityMetadataManager entityMetadataManager,IMutableEntityType mutableEntityType, ModelBuilder modelBuilder, string tail)
        {
            var clrType = mutableEntityType.ClrType;
            var entityMetadata = entityMetadataManager.TryGet(clrType);
            var shardingEntity = entityMetadata.EntityType;
            var tableSeparator = entityMetadata.TableSeparator;
            var entity = modelBuilder.Entity(shardingEntity);
            var tableName = entityMetadata.LogicTableName;
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException($"{shardingEntity}: not found logic table name。");
            entity.ToTable($"{tableName}{tableSeparator}{tail}");
        }
    }
}