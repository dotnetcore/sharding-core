using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.UnionAllMergeShardingProviders.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace Sample.SqlServer.UnionAllMerge
{
    public class UnionAllMergeSqlServerQuerySqlGeneratorFactory<TShardingDbContext> : IQuerySqlGeneratorFactory, IUnionAllMergeQuerySqlGeneratorFactory
    where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IShardingRuntimeContext _shardingRuntimeContext;

        public UnionAllMergeSqlServerQuerySqlGeneratorFactory(QuerySqlGeneratorDependencies dependencies,IShardingRuntimeContext shardingRuntimeContext)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
            Dependencies = dependencies;
        }

        public QuerySqlGeneratorDependencies Dependencies { get; }
        public QuerySqlGenerator Create() => new UnionAllMergeSqlServerQuerySqlGenerator<TShardingDbContext>(Dependencies,_shardingRuntimeContext);
    }

    public class UnionAllMergeSqlServerQuerySqlGenerator<TShardingDbContext> : SqlServerQuerySqlGenerator
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IShardingRuntimeContext _shardingRuntimeContext;
        private readonly IEntityMetadataManager _entityMetadataManager;
        private readonly IUnionAllMergeManager _unionAllMergeManager;

        public UnionAllMergeSqlServerQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies,IShardingRuntimeContext shardingRuntimeContext)
            : base(dependencies)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
            _entityMetadataManager = shardingRuntimeContext.GetEntityMetadataManager();
            _unionAllMergeManager = shardingRuntimeContext.GetRequiredService<IUnionAllMergeManager>();
        }

        protected override Expression VisitTable(TableExpression tableExpression)
        {
            return OverrideVisitTable(tableExpression);
            // this._relationalCommandBuilder.Append((object) this._sqlGenerationHelper.DelimitIdentifier(tableExpression.Name, tableExpression.Schema)).Append((object) this.AliasSeparator).Append((object) this._sqlGenerationHelper.DelimitIdentifier(tableExpression.Alias));
            // return (Expression) tableExpression;

            // typeof(TableExpression)
            //     .GetFields( BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(o=>o.Name.Contains(nameof(tableExpression.Name)))
            //     .SetValue(tableExpression,"(select * from Log_1Message union all select * from Log_1Message)");

            // base will append schema, table and alias
        }

        private Expression OverrideVisitTable(TableExpression tableExpression)
        {
            if (_unionAllMergeManager?.Current != null)
            {
                var entityMetadatas = _entityMetadataManager.TryGetByLogicTableName(tableExpression.Name);
                var tableRouteResults = _unionAllMergeManager?.Current.TableRoutesResults.ToArray();
                if (tableRouteResults.IsNotEmpty() &&
                    entityMetadatas.IsNotEmpty()&&
                    entityMetadatas.Count==1&&
                    tableRouteResults[0].ReplaceTables.Any(o =>o.EntityType==entityMetadatas[0].EntityType));
                {
                    var tails = tableRouteResults.Select(o => o.ReplaceTables.FirstOrDefault(r => r.EntityType==entityMetadatas[0].EntityType)?.Tail).ToHashSet();

                    var sqlGenerationHelper = typeof(QuerySqlGenerator).GetTypeFieldValue(this, "_sqlGenerationHelper") as ISqlGenerationHelper;
                    string newTableName = null;
                    if (tails.Count == 1)
                    {
                        newTableName = sqlGenerationHelper.DelimitIdentifier($"{tableExpression.Name}{entityMetadatas[0].TableSeparator}{tails.First()}", tableExpression.Schema);
                    }
                    else
                    {
                        newTableName = "(" + string.Join(" union all ", tails.Select(tail => $"select * from {sqlGenerationHelper.DelimitIdentifier($"{tableExpression.Name}{entityMetadatas[0].TableSeparator}{tail}", tableExpression.Schema)}")) + ")";
                    }

                    var relationalCommandBuilder = typeof(QuerySqlGenerator).GetTypeFieldValue(this, "_relationalCommandBuilder") as IRelationalCommandBuilder;
                    relationalCommandBuilder.Append(newTableName).Append(this.AliasSeparator).Append(sqlGenerationHelper.DelimitIdentifier(tableExpression.Alias));
                    return tableExpression;
                }
            }

            var result = base.VisitTable(tableExpression);
            return result;
        }
    }
}
