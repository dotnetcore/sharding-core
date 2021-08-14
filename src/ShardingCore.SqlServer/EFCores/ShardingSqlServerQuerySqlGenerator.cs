#if !EFCORE2
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Extensions;

namespace ShardingCore.SqlServer.EFCores
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 22 December 2020 09:47:59
* @Email: 326308290@qq.com
*/
    public class ShardingSqlServerQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
    {
        public ShardingSqlServerQuerySqlGeneratorFactory(QuerySqlGeneratorDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        public QuerySqlGeneratorDependencies Dependencies { get; }
        public QuerySqlGenerator Create() => new ShardingSqlServerQuerySqlGenerator(Dependencies);
    }

    public class ShardingSqlServerQuerySqlGenerator : SqlServerQuerySqlGenerator
    {
        public ShardingSqlServerQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies)
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
            var shardingAccessor = ShardingContainer.Services.GetService<IShardingAccessor>();
            if (shardingAccessor?.ShardingContext != null)
            {
                var virtualTableManager = ShardingContainer.Services.GetService<IVirtualTableManager>();
                var virtualTable = virtualTableManager.GetAllVirtualTables().FirstOrDefault(o => o.GetOriginalTableName() == tableExpression.Name);
                if(virtualTable!=null)
                {
                    var tails = shardingAccessor.ShardingContext.GetContextQueryTails(virtualTable);
                    var tailPrefix = virtualTable.ShardingConfig.TailPrefix;
                    string newTableName = null;
                    var sqlGenerationHelper = typeof(QuerySqlGenerator).GetTypeFieldValue(this, "_sqlGenerationHelper") as ISqlGenerationHelper;

                    if (tails.IsEmpty())
                    {
                        var firstTail = virtualTableManager.GetVirtualTable(tableExpression.Name).GetAllPhysicTables()[0].Tail;
                        newTableName = $"( select * from {sqlGenerationHelper.DelimitIdentifier($"{tableExpression.Name}{tailPrefix}{firstTail}", tableExpression.Schema)} where 1=2 )";
                    }
                    else if (tails.Count == 1)
                    {
                        //对tableExpresion进行重写
                        //typeof(TableExpression)
                        //    .GetFields( BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(o=>o.Name==$"<{nameof(tableExpression.Name)}>k__BackingField")
                        //    .SetValue(tableExpression,$"{tableExpression.Name}{tailPrefix}{entry.Tails[0]}");
                        newTableName = sqlGenerationHelper.DelimitIdentifier($"{tableExpression.Name}{tailPrefix}{tails[0]}", tableExpression.Schema);
                    }
                    else
                    {
                        newTableName = "(" + string.Join(" union all ", tails.Select(tail => $"select * from {sqlGenerationHelper.DelimitIdentifier($"{tableExpression.Name}{tailPrefix}{tail}", tableExpression.Schema)}")) + ")";
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
#endif

#if EFCORE2
using System.Linq;
using System.Linq.Expressions;
using ShardingCore;
using ShardingCore.Extensions;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Core.VirtualTables;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace ShardingCore.SqlServer.EFCores
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 22 December 2020 09:47:59
* @Email: 326308290@qq.com
*/
    public class ShardingSqlServerQuerySqlGeneratorFactory :QuerySqlGeneratorFactoryBase
    {
        
        private readonly ISqlServerOptions _sqlServerOptions;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ShardingSqlServerQuerySqlGeneratorFactory(
           QuerySqlGeneratorDependencies dependencies,
           ISqlServerOptions sqlServerOptions)
            : base(dependencies)
        {
            _sqlServerOptions = sqlServerOptions;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IQuerySqlGenerator CreateDefault(SelectExpression selectExpression)
            => new ShardingSqlServerQuerySqlGenerator(
                Dependencies,
                selectExpression,
                _sqlServerOptions.RowNumberPagingEnabled);
    }

    public class ShardingSqlServerQuerySqlGenerator : SqlServerQuerySqlGenerator
    {

        public ShardingSqlServerQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies, SelectExpression selectExpression, bool rowNumberPagingEnabled) : base(dependencies, selectExpression, rowNumberPagingEnabled)
        {
        }

        public override Expression VisitTable(TableExpression tableExpression)
        {
            return OverrideVisitTable(tableExpression);
        }
        // protected override Expression VisitTable(TableExpression tableExpression)
        // {
        //     return OverrideVisitTable(tableExpression);
        //     // this._relationalCommandBuilder.Append((object) this._sqlGenerationHelper.DelimitIdentifier(tableExpression.Name, tableExpression.Schema)).Append((object) this.AliasSeparator).Append((object) this._sqlGenerationHelper.DelimitIdentifier(tableExpression.Alias));
        //     // return (Expression) tableExpression;
        //
        //     // typeof(TableExpression)
        //     //     .GetFields( BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(o=>o.Name.Contains(nameof(tableExpression.Name)))
        //     //     .SetValue(tableExpression,"(select * from Log_1Message union all select * from Log_1Message)");
        //
        //     // base will append schema, table and alias
        // }

        private Expression OverrideVisitTable(TableExpression tableExpression)
        {
            var shardingAccessor = ShardingContainer.Services.GetService<IShardingAccessor>();
            if (shardingAccessor?.ShardingContext != null)
            {
                var virtualTableManager = ShardingContainer.Services.GetService<IVirtualTableManager>();
                var virtualTable = virtualTableManager.GetAllVirtualTables().FirstOrDefault(o => o.GetOriginalTableName() == tableExpression.Table);
                if(virtualTable!=null)
                {
                    var tails = shardingAccessor.ShardingContext.GetContextQueryTails(virtualTable);
                    var tailPrefix = virtualTable.ShardingConfig.TailPrefix;
                    string newTableName = null;
                    var sqlGenerationHelper = typeof(DefaultQuerySqlGenerator).GetTypeFieldValue(this, "_sqlGenerationHelper") as ISqlGenerationHelper;

                    if (tails.IsEmpty())
                    {
                        var firstTail = virtualTableManager.GetVirtualTable(tableExpression.Table).GetAllPhysicTables()[0].Tail;
                        newTableName = $"( select * from {sqlGenerationHelper.DelimitIdentifier($"{tableExpression.Table}{tailPrefix}{firstTail}", tableExpression.Schema)} where 1=2 )";
                    }
                    else if (tails.Count == 1)
                    {
                        //对tableExpresion进行重写
                        //typeof(TableExpression)
                        //    .GetFields( BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(o=>o.Name==$"<{nameof(tableExpression.Name)}>k__BackingField")
                        //    .SetValue(tableExpression,$"{tableExpression.Name}{tailPrefix}{entry.Tails[0]}");
                        newTableName = sqlGenerationHelper.DelimitIdentifier($"{tableExpression.Table}{tailPrefix}{tails[0]}", tableExpression.Schema);
                    }
                    else
                    {
                        newTableName = "(" + string.Join(" union all ", tails.Select(tail => $"select * from {sqlGenerationHelper.DelimitIdentifier($"{tableExpression.Table}{tailPrefix}{tail}", tableExpression.Schema)}")) + ")";
                    }

                    var relationalCommandBuilder = typeof(DefaultQuerySqlGenerator).GetTypeFieldValue(this, "_relationalCommandBuilder") as IRelationalCommandBuilder;
                    relationalCommandBuilder.Append(newTableName).Append(this.AliasSeparator).Append(sqlGenerationHelper.DelimitIdentifier(tableExpression.Alias));
                    return tableExpression;
                }
            }

            var result = base.VisitTable(tableExpression);
            return result;
        }
    }
}
#endif