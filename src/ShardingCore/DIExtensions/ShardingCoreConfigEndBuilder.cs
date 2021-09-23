using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.DIExtensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/19 22:36:20
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingCoreConfigEndBuilder<TShardingDbContext, TActualDbContext>
        where TActualDbContext : DbContext
        where TShardingDbContext : DbContext, IShardingDbContext<TActualDbContext>
    {
        private readonly ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext> _shardingCoreConfigBuilder;

        public ShardingCoreConfigEndBuilder(
            ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext> shardingCoreConfigBuilder)
        {
            _shardingCoreConfigBuilder = shardingCoreConfigBuilder;
        }

        public IServiceCollection End()
        {
            var services = _shardingCoreConfigBuilder.Services;
            services.AddSingleton<IShardingConfigOption, ShardingConfigOption<TShardingDbContext>>(sp =>
                _shardingCoreConfigBuilder.ShardingConfigOption);


            //添加创建TActualDbContext 的DbContextOptionsBuilder创建者
            var config = new ShardingDbContextOptionsBuilderConfig<TShardingDbContext>(
                _shardingCoreConfigBuilder.ShardingConfigOption.SameConnectionConfigure,
                _shardingCoreConfigBuilder.ShardingConfigOption.DefaultQueryConfigure);
            services
                .AddSingleton<IShardingDbContextOptionsBuilderConfig<TShardingDbContext>,
                    ShardingDbContextOptionsBuilderConfig<TShardingDbContext>>(sp => config);

            //添加创建TActualDbContext创建者
            services
                .AddSingleton<IShardingDbContextCreatorConfig,
                    DefaultShardingDbContextCreatorConfig<TShardingDbContext, TActualDbContext>>(sp =>
                    new DefaultShardingDbContextCreatorConfig<TShardingDbContext, TActualDbContext>(
                        typeof(TActualDbContext)));

            if (!_shardingCoreConfigBuilder.ShardingConfigOption.UseReadWrite)
            {
                services.AddTransient<IConnectionStringManager<TShardingDbContext>, DefaultConnectionStringManager<TShardingDbContext>>();
            }
            else
            {
                services.AddTransient<IConnectionStringManager<TShardingDbContext>, ReadWriteConnectionStringManager<TShardingDbContext>>();

                services.AddSingleton<IReadWriteOptions, ReadWriteOptions<TShardingDbContext>>(sp =>
                    new ReadWriteOptions<TShardingDbContext>(
                        _shardingCoreConfigBuilder.ShardingConfigOption.ReadWriteDefaultPriority,
                        _shardingCoreConfigBuilder.ShardingConfigOption.ReadWriteDefaultEnable,
                        _shardingCoreConfigBuilder.ShardingConfigOption.ReadConnStringGetStrategy));
                if (_shardingCoreConfigBuilder.ShardingConfigOption.ReadStrategyEnum == ReadStrategyEnum.Loop)
                {
                    services
                        .AddSingleton<IShardingConnectionStringResolver,
                            LoopShardingConnectionStringResolver<TShardingDbContext>>(sp =>
                        {

                            var readConnString = _shardingCoreConfigBuilder.ShardingConfigOption.ReadConnStringConfigure(sp);
                            var readWriteLoopConnectors = readConnString.Select(o => new ReadWriteLoopConnector(o.Key, o.Value));

                            return new LoopShardingConnectionStringResolver<TShardingDbContext>(
                                readWriteLoopConnectors);
                        });
                }
                else if (_shardingCoreConfigBuilder.ShardingConfigOption.ReadStrategyEnum == ReadStrategyEnum.Random)
                {
                    services
                        .AddSingleton<IShardingConnectionStringResolver,
                            RandomShardingConnectionStringResolver<TShardingDbContext>>(sp =>
                        {
                            var readConnString = _shardingCoreConfigBuilder.ShardingConfigOption.ReadConnStringConfigure(sp);
                            var readWriteRandomConnectors = readConnString.Select(o => new ReadWriteRandomConnector(o.Key, o.Value));
                            return new RandomShardingConnectionStringResolver<TShardingDbContext>(
                                readWriteRandomConnectors);
                        });
                }


                services.TryAddSingleton<IShardingReadWriteManager, ShardingReadWriteManager>();
                services.AddSingleton<IShardingReadWriteAccessor, ShardingReadWriteAccessor<TShardingDbContext>>();
                //foreach (var dataSourceKv in dataSources)
                //{
                //    if (dataSourceKv.Key == _shardingCoreConfigBuilder.DefaultDataSourceName)
                //        throw new InvalidOperationException($"{nameof(AddShardingDataSource)} include default data source name:{_shardingCoreConfigBuilder.DefaultDataSourceName}");
                //    _shardingCoreConfigBuilder.AddShardingDataSource.Add(dataSourceKv.Key, dataSourceKv.Value);
                //}
            }
            services.AddInternalShardingCore();

            return services;

        }
    }
}