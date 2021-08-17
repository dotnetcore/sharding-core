using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.EFCores;
using ShardingCore.Sharding;
using ShardingCore.SqlServer.EFCores;
using System;

#if EFCORE2
using Microsoft.EntityFrameworkCore.Query.Sql;
#endif

namespace ShardingCore.SqlServer
{
    /*
    * @Author: xjm
    * @Description: 
    * @Date: 2020年4月7日 9:30:18
    * @Email: 326308290@qq.com
    */
    public static class DIExtension
    {
        public static IServiceCollection AddShardingSqlServer(this IServiceCollection services, Action<SqlServerOptions> configure)
        {
            if (configure == null)
                throw new ArgumentNullException($"AddScfSqlServerProvider 参数不能为空:{nameof(configure)}");
           
            var options = new SqlServerOptions();
            configure(options);
            services.AddSingleton<IShardingCoreOptions, SqlServerOptions>(sp=> options);
            services.AddShardingCore();

            services.AddSingleton<IDbContextOptionsProvider, SqlServerDbContextOptionsProvider>();
            services.AddSingleton<IShardingParallelDbContextFactory, ShardingSqlServerParallelDbContextFactory>();
          
            services.AddSingleton<IShardingBootstrapper,ShardingBootstrapper>();
            return services;
        }

        internal static DbContextOptionsBuilder UseShardingSqlServerQuerySqlGenerator(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IQuerySqlGeneratorFactory, ShardingSqlServerQuerySqlGeneratorFactory>();
            return optionsBuilder;
        }
        internal static DbContextOptionsBuilder<TContext> UseShardingSqlServerQuerySqlGenerator<TContext>(this DbContextOptionsBuilder<TContext> optionsBuilder) where TContext : DbContext
        {
            optionsBuilder.ReplaceService<IQuerySqlGeneratorFactory, ShardingSqlServerQuerySqlGeneratorFactory>();
            return optionsBuilder;
        }


        public static DbContextOptionsBuilder UseSharding(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IDbSetSource, ShardingDbSetSource>().ReplaceService<IQueryCompiler, ShardingQueryCompiler>();
            return optionsBuilder;
        }
    }
}