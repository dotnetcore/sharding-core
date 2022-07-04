using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Pomelo.EntityFrameworkCore.MySql.Migrations;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Helpers;

namespace Sample.MySql
{
    public class ShardingMySqlMigrationsSqlGenerator:MySqlMigrationsSqlGenerator
    {
        private readonly IShardingRuntimeContext _shardingRuntimeContext;

        public ShardingMySqlMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IRelationalAnnotationProvider annotationProvider, IMySqlOptions options,IShardingRuntimeContext shardingRuntimeContext) : base(dependencies, annotationProvider, options)
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