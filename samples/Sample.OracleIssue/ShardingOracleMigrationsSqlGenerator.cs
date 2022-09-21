using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Oracle.EntityFrameworkCore.Infrastructure.Internal;
using Oracle.EntityFrameworkCore.Migrations;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Helpers;

namespace Sample.OracleIssue;

public class ShardingOracleMigrationsSqlGenerator:OracleMigrationsSqlGenerator
{
    private readonly IShardingRuntimeContext _shardingRuntimeContext;

    public ShardingOracleMigrationsSqlGenerator(IShardingRuntimeContext shardingRuntimeContext,MigrationsSqlGeneratorDependencies dependencies, IOracleOptions options) : base(dependencies, options)
    {
        _shardingRuntimeContext = shardingRuntimeContext;
    }

    protected override void Generate(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        var oldCmds = builder.GetCommandList().ToList();
        base.Generate(operation, model, builder);
        var newCmds = builder.GetCommandList().ToList();
        var addCmds = newCmds.Where(x => !oldCmds.Contains(x)).ToList();

        //oracle需要自行重写因为和其他的数据库不一样并不是使用了基类来处理
        OracleMigrationHelper.Generate(_shardingRuntimeContext,operation, builder, Dependencies.SqlGenerationHelper, addCmds);
    }
}