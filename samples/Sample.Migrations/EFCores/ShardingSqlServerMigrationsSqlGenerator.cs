using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;

namespace Sample.Migrations.EFCores
{
    /// <summary>
    /// https://github.com/Coldairarrow/EFCore.Sharding/blob/master/src/EFCore.Sharding.SqlServer/ShardingSqlServerMigrationsSqlGenerator.cs
    /// </summary>
    public class ShardingSqlServerMigrationsSqlGenerator : SqlServerMigrationsSqlGenerator
    {
        private readonly IShardingRuntimeContext _shardingRuntimeContext;

        public ShardingSqlServerMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IRelationalAnnotationProvider migrationsAnnotations,IShardingRuntimeContext shardingRuntimeContext) : base(dependencies, migrationsAnnotations)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
        }
        protected override void Generate(
            MigrationOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            var oldCmds = builder.GetCommandList().ToList();
            base.Generate(operation, model, builder);
            var newCmds = builder.GetCommandList().ToList();
            var addCmds = newCmds.Where(x => !oldCmds.Contains(x)).ToList();

            MigrationHelper.Generate(_shardingRuntimeContext,operation, builder, Dependencies.SqlGenerationHelper, addCmds);
        }
    }
}
