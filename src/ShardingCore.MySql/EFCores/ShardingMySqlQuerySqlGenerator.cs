#if !EFCORE2
using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Pomelo.EntityFrameworkCore.MySql.Query.ExpressionVisitors.Internal;
using Pomelo.EntityFrameworkCore.MySql.Query.Internal;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Extensions;

namespace ShardingCore.MySql.EFCores
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 22 December 2020 09:47:59
* @Email: 326308290@qq.com
*/
    public class ShardingMySqlQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
    {
        
        private readonly QuerySqlGeneratorDependencies _dependencies;
        private readonly MySqlSqlExpressionFactory _sqlExpressionFactory;
        private readonly IMySqlOptions _options;

        public ShardingMySqlQuerySqlGeneratorFactory(
            QuerySqlGeneratorDependencies dependencies,
            ISqlExpressionFactory sqlExpressionFactory,
            IMySqlOptions options)
        {
            this._dependencies = dependencies;
            this._sqlExpressionFactory = (MySqlSqlExpressionFactory) sqlExpressionFactory;
            this._options = options;
        }

        public virtual QuerySqlGenerator Create() => (QuerySqlGenerator) new ShardingMySqlQuerySqlGenerator(this._dependencies, this._sqlExpressionFactory, this._options);
    }

    public class ShardingMySqlQuerySqlGenerator :MySqlQuerySqlGenerator
    {
        public ShardingMySqlQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies, MySqlSqlExpressionFactory sqlExpressionFactory, IMySqlOptions options) : base(dependencies, sqlExpressionFactory, options)
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
                var virtualTable = virtualTableManager.GetAllVirtualTables(shardingAccessor.ShardingContext.ConnectKey).FirstOrDefault(o => o.GetOriginalTableName() == tableExpression.Name);
                if(virtualTable!=null)
                {
                    var tails = shardingAccessor.ShardingContext.GetContextQueryTails(virtualTable);
                    var tailPrefix = virtualTable.ShardingConfig.TailPrefix;
                    string newTableName = null;
                    var sqlGenerationHelper = typeof(QuerySqlGenerator).GetTypeFieldValue(this, "_sqlGenerationHelper") as ISqlGenerationHelper;

                    if (tails.IsEmpty())
                    {
                        var firstTail = virtualTableManager.GetVirtualTable(shardingAccessor.ShardingContext.ConnectKey,tableExpression.Name).GetAllPhysicTables()[0].Tail;
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
using System;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore;
using ShardingCore.Extensions;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Core.VirtualTables;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Pomelo.EntityFrameworkCore.MySql.Query.Sql.Internal;

namespace ShardingCore.MySql.EFCores
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 22 December 2020 09:47:59
* @Email: 326308290@qq.com
*/
    public class ShardingMySqlQuerySqlGeneratorFactory :QuerySqlGeneratorFactoryBase
    {
        
        private readonly IMySqlOptions _options;

        public ShardingMySqlQuerySqlGeneratorFactory(QuerySqlGeneratorDependencies dependencies, IMySqlOptions options)
            : base(dependencies)
        {
            _options = options;
        }

        public override IQuerySqlGenerator CreateDefault(SelectExpression selectExpression)
            => new ShardingMySqlQuerySqlGenerator(
                Dependencies,
                selectExpression, _options);
    }

    public class ShardingMySqlQuerySqlGenerator : MySqlQuerySqlGenerator
    {

        public ShardingMySqlQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies, SelectExpression selectExpression, IMySqlOptions options) : base(dependencies, selectExpression, options)
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
                var virtualTable = virtualTableManager.GetAllVirtualTables(shardingAccessor.ShardingContext.ConnectKey).FirstOrDefault(o => o.GetOriginalTableName() == tableExpression.Table);
                if(virtualTable!=null)
                {
                    var tails = shardingAccessor.ShardingContext.GetContextQueryTails(virtualTable);
                    var tailPrefix = virtualTable.ShardingConfig.TailPrefix;
                    string newTableName = null;
                    var sqlGenerationHelper = typeof(DefaultQuerySqlGenerator).GetTypePropertyValue(this, "SqlGenerator") as ISqlGenerationHelper;

                    if (tails.IsEmpty())
                    {
                        var firstTail = virtualTableManager.GetVirtualTable(shardingAccessor.ShardingContext.ConnectKey,tableExpression.Table).GetAllPhysicTables()[0].Tail;
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