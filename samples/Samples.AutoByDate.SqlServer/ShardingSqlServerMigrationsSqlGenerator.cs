using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Samples.AutoByDate.SqlServer.Domain.Entities;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;

namespace Samples.AutoByDate.SqlServer
{
    public class ShardingIgnoreSqlServerMigrationsSqlGenerator<TShardingDbContext> : SqlServerMigrationsSqlGenerator
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private ISet<string> _ignoreForeignKeyTables = new HashSet<string>()
        {
            nameof(SysUserLog1ByDay)
        };
        public ShardingIgnoreSqlServerMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IRelationalAnnotationProvider migrationsAnnotations) : base(dependencies, migrationsAnnotations)
        {
        }

        protected override void Generate(CreateTableOperation operation, IModel? model, MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            Console.WriteLine("123123");
            base.Generate(operation, model, builder, terminate);
        }

        protected override void Generate(DropForeignKeyOperation operation, IModel? model, MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            if (_ignoreForeignKeyTables.Contains(operation.Table))
            {
                return;
            }
            base.Generate(operation, model, builder, terminate);
        }

        protected override void Generate(AddForeignKeyOperation operation, IModel? model, MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            if (_ignoreForeignKeyTables.Contains(operation.Table))
            {
                return;
            }
            base.Generate(operation, model, builder, terminate);
        }
    }
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
