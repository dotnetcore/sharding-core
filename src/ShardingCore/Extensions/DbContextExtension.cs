using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ShardingCore.Extensions
{
    public static class DbContextExtension
    {
        public static void RemoveDbContextRelationModelThatIsShardingTable(this DbContext dbContext)
        {
            var contextModel = dbContext.Model as Model;
            var contextModelRelationalModel = contextModel.RelationalModel as RelationalModel;
            var valueTuples = contextModelRelationalModel.Tables.Where(o=>o.Value.EntityTypeMappings.Any(m=>m.EntityType.ClrType.IsShardingTable())).Select(o=>o.Key).ToList();
            for (int i = 0; i < valueTuples.Count; i++)
            {
                contextModelRelationalModel.Tables.Remove(valueTuples[i]);
            }
        }
        public static void RemoveDbContextRelationModelSaveOnlyThatIsShardingTable(this DbContext dbContext,Type shardingType)
        {
            var contextModel = dbContext.Model as Model;
            var contextModelRelationalModel = contextModel.RelationalModel as RelationalModel;
            var valueTuples = contextModelRelationalModel.Tables.Where(o=> o.Value.EntityTypeMappings.All(m => m.EntityType.ClrType != shardingType)).Select(o=>o.Key).ToList();
            for (int i = 0; i < valueTuples.Count; i++)
            {
                contextModelRelationalModel.Tables.Remove(valueTuples[i]);
            }
        }
    }
}