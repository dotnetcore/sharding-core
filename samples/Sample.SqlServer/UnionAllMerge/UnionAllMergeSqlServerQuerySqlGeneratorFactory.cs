using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore;
using ShardingCore.Core.UnionAllMergeShardingProviders.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace Sample.SqlServer.UnionAllMerge
{
    public class UnionAllMergeSqlServerQuerySqlGeneratorFactory<TShardingDbContext> : IQuerySqlGeneratorFactory, IUnionAllMergeQuerySqlGeneratorFactory
    where TShardingDbContext : DbContext, IShardingDbContext
    {
        public UnionAllMergeSqlServerQuerySqlGeneratorFactory(QuerySqlGeneratorDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        public QuerySqlGeneratorDependencies Dependencies { get; }
        public QuerySqlGenerator Create() => new UnionAllMergeSqlServerQuerySqlGenerator<TShardingDbContext>(Dependencies);
    }

    public class UnionAllMergeSqlServerQuerySqlGenerator<TShardingDbContext> : SqlServerQuerySqlGenerator
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        public UnionAllMergeSqlServerQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies)
            : base(dependencies)
        {
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
            var supportManager = ShardingContainer.GetService<IUnionAllMergeManager>();
            if (supportManager?.Current != null)
            {
                var tableRouteResults = supportManager?.Current.TableRoutesResults.ToArray();
                if (tableRouteResults.IsNotEmpty() &&
                    tableRouteResults[0].ReplaceTables.Any(o => o.OriginalName == tableExpression.Name))
                {
                    var tails = tableRouteResults.Select(o => o.ReplaceTables.FirstOrDefault(r => r.OriginalName == tableExpression.Name).Tail).ToHashSet();

                    var sqlGenerationHelper = typeof(QuerySqlGenerator).GetTypeFieldValue(this, "_sqlGenerationHelper") as ISqlGenerationHelper;
                    var tableManager = ShardingContainer.GetService<IVirtualTableManager<TShardingDbContext>>();
                    var virtualTable = tableManager.GetVirtualTable(tableExpression.Name);
                    string newTableName = null;
                    if (tails.Count == 1)
                    {
                        newTableName = sqlGenerationHelper.DelimitIdentifier($"{tableExpression.Name}{virtualTable.EntityMetadata.TableSeparator}{tails.First()}", tableExpression.Schema);
                    }
                    else
                    {
                        newTableName = "(" + string.Join(" union all ", tails.Select(tail => $"select * from {sqlGenerationHelper.DelimitIdentifier($"{tableExpression.Name}{virtualTable.EntityMetadata.TableSeparator}{tail}", tableExpression.Schema)}")) + ")";
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
