using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Extensions;
using ShardingCore.MySql.EFCores;
using ShardingCore.TableCreator;
#if EFCORE2
using Microsoft.EntityFrameworkCore.Query.Sql;
#endif
#if !EFCORE2
using Microsoft.EntityFrameworkCore.Query;
#endif


namespace ShardingCore.MySql
{
    /*
    * @Author: xjm
    * @Description: 
    * @Date: 2020年4月7日 9:30:18
    * @Email: 326308290@qq.com
    */
    public static class DIExtension
    {
        public static IServiceCollection AddShardingMySql(this IServiceCollection services, Action<MySqlOptions> configure)
        {
            if (configure == null)
                throw new ArgumentNullException($"AddScfSqlServerProvider :{nameof(configure)}");
           
            var options = new MySqlOptions();
            configure(options);
            services.AddSingleton<MySqlOptions>(options);
            services.AddSingleton<IShardingCoreOptions, MySqlOptions>(sp => options);
            services.AddShardingCore();

            services.AddSingleton<IDbContextOptionsProvider, MySqlDbContextOptionsProvider>();
            services.AddSingleton<IShardingParallelDbContextFactory, ShardingMySqlParallelDbContextFactory>();
          
            services.AddSingleton<IShardingBootstrapper,ShardingBootstrapper>();
            return services;
        }

        internal static DbContextOptionsBuilder UseShardingMySqlQuerySqlGenerator(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IQuerySqlGeneratorFactory, ShardingMySqlQuerySqlGeneratorFactory>();
            return optionsBuilder;
        }
        internal static DbContextOptionsBuilder<TContext> UseShardingMySqlQuerySqlGenerator<TContext>(this DbContextOptionsBuilder<TContext> optionsBuilder) where TContext:DbContext
        {
            optionsBuilder.ReplaceService<IQuerySqlGeneratorFactory, ShardingMySqlQuerySqlGeneratorFactory>();
            return optionsBuilder;
        }
        public static DbContextOptionsBuilder UseShardingMySqlUpdateSqlGenerator(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IUpdateSqlGenerator, ShardingMySqlUpdateSqlGenerator>();
            return optionsBuilder;
        }
        public static DbContextOptionsBuilder<TContext> UseShardingMySqlUpdateSqlGenerator<TContext>(this DbContextOptionsBuilder<TContext> optionsBuilder) where TContext:DbContext
        {
            optionsBuilder.ReplaceService<IUpdateSqlGenerator, ShardingMySqlUpdateSqlGenerator>();
            return optionsBuilder;
        }
    }
}