using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Core.VirtualRoutes;
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
            services.AddSingleton(options);
            services.AddShardingCore();

            services.AddScoped<IDbContextOptionsProvider, MySqlDbContextOptionsProvider>();
            services.AddSingleton<IShardingParallelDbContextFactory, ShardingMySqlParallelDbContextFactory>();
            if (options.HasSharding)
            {
                foreach (var shardingRoute in options.ShardingRoutes)
                {
                    var genericVirtualRoute = shardingRoute.GetInterfaces().FirstOrDefault(it => it.IsInterface && it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IVirtualRoute<>)
                                                                                                                                                                                && it.GetGenericArguments().Any());
                    if (genericVirtualRoute == null)
                        throw new ArgumentException("add sharding route type error not assignable from IVirtualRoute<>.");
                    var shardingEntity=genericVirtualRoute.GetGenericArguments()[0];
                    if(!shardingEntity.IsShardingEntity())
                        throw new ArgumentException("add sharding route type error generic arguments first not assignable from IShardingEntity.");
                    Type genericType = typeof(IVirtualRoute<>);
                    Type interfaceType = genericType.MakeGenericType(shardingEntity);
                    services.AddSingleton(interfaceType, shardingRoute);
                }
            }
            services.AddSingleton(sp =>
            {
                var shardingCoreConfig = new ShardingCoreConfig();
                options.ShardingCoreConfigConfigure?.Invoke(sp,shardingCoreConfig);
                return shardingCoreConfig;
            });
            services.AddSingleton<IShardingBootstrapper,ShardingBootstrapper>();
            return services;
        }

        public static DbContextOptionsBuilder UseShardingSqlServerQuerySqlGenerator(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IQuerySqlGeneratorFactory, ShardingMySqlQuerySqlGeneratorFactory>();
            return optionsBuilder;
        }
    }
}