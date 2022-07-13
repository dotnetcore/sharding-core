using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using System.Linq;

namespace WebApplication1.Data
{
    /// <summary>
    /// https://github.com/Coldairarrow/EFCore.Sharding/blob/master/src/EFCore.Sharding.SqlServer/ShardingSqlServerMigrationsSqlGenerator.cs
    /// </summary>
    public class ShardingMigrationsSqlGenerator<TShardingDbContext> : NpgsqlMigrationsSqlGenerator where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IShardingRuntimeContext runtimeContext;

        public ShardingMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, INpgsqlSingletonOptions npgsqlSingletonOptions, IShardingRuntimeContext runtimeContext) : base(dependencies, npgsqlSingletonOptions)
        {
            this.runtimeContext = runtimeContext;
        }

        protected override void Generate(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            var oldCmds = builder.GetCommandList().ToList();
            base.Generate(operation, model, builder);
            var newCmds = builder.GetCommandList().ToList();
            var addCmds = newCmds.Where(x => !oldCmds.Contains(x)).ToList();

            MigrationHelper.Generate(runtimeContext, operation, builder, Dependencies.SqlGenerationHelper, addCmds);
        }
    }

}
