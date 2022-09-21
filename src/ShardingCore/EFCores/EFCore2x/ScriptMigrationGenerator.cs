#if EFCORE2
using Microsoft.EntityFrameworkCore.Migrations;
using ShardingCore.Core.RuntimeContexts;

namespace ShardingCore.EFCores
{
    public sealed class ScriptMigrationGenerator:AbstractScriptMigrationGenerator
    {
        private readonly string _fromMigration;
        private readonly string _toMigration;
        private readonly bool _idempotent;

        public ScriptMigrationGenerator(IShardingRuntimeContext shardingRuntimeContext, string fromMigration = null,
            string toMigration = null, bool idempotent = false) : base(shardingRuntimeContext)
        {
            _fromMigration = fromMigration;
            _toMigration = toMigration;
            _idempotent = idempotent;
        }

        protected override string GenerateScriptSql(IMigrator migrator)
        {
            return migrator.GenerateScript(_fromMigration, _toMigration, _idempotent);
        }
    }
}
#endif