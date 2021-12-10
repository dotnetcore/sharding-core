using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShardingCore.Exceptions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;
using ShardingCore.Sharding.ShardingComparision;
using ShardingCore.Sharding.ShardingComparision.Abstractions;

namespace ShardingCore.DIExtensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/19 22:36:20
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingCoreConfigEndBuilder<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly ShardingCoreConfigBuilder<TShardingDbContext> _shardingCoreConfigBuilder;

        public ShardingCoreConfigEndBuilder(
            ShardingCoreConfigBuilder<TShardingDbContext> shardingCoreConfigBuilder)
        {
            _shardingCoreConfigBuilder = shardingCoreConfigBuilder;
        }

        /// <summary>
        /// 替换比较表达式
        /// 比较表达式用于将数据库的数据获取到内存后进行排序，由于数据库排序和内存排序针对某种类型可能不一致导致结果和预期不符，如guid和unique identifier
        /// 默认已经将此类型的比较器已经修复如果有后续其他数据库类型和c#类型排序不一致的请自行实现 <see cref="IShardingComparer"/>
        /// </summary>
        /// <param name="newShardingComparerFactory"></param>
        /// <returns></returns>
        public ShardingCoreConfigEndBuilder<TShardingDbContext>  ReplaceShardingComparer(Func<IServiceProvider, IShardingComparer<TShardingDbContext>> newShardingComparerFactory)
        {
            _shardingCoreConfigBuilder.ShardingConfigOption.ReplaceShardingComparer(newShardingComparerFactory);
            return this;
        }
        /// <summary>
        /// 是否启用读写分离
        /// </summary>
        protected bool UseReadWrite => _shardingCoreConfigBuilder.ShardingConfigOption.UseReadWrite;
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

            if (_shardingCoreConfigBuilder.ShardingConfigOption.ReplaceShardingComparerFactory == null)
            {
                throw new ShardingCoreConfigException($"{nameof(_shardingCoreConfigBuilder.ShardingConfigOption.ReplaceShardingComparerFactory)}  is null");
            }
            services.AddSingleton<IShardingComparer<TShardingDbContext>>(_shardingCoreConfigBuilder.ShardingConfigOption.ReplaceShardingComparerFactory);


            if (!UseReadWrite)
            {
                services.AddTransient<IConnectionStringManager<TShardingDbContext>, DefaultConnectionStringManager<TShardingDbContext>>();
            }
            else
            {
                services.AddTransient<IConnectionStringManager<TShardingDbContext>, ReadWriteConnectionStringManager<TShardingDbContext>>();
                RegisterReadWriteConfigure(services);
            }
            services.AddInternalShardingCore();

            return services;

        }
        /// <summary>
        /// 配置读写分离
        /// </summary>
        /// <param name="services"></param>
        /// <exception cref="ShardingCoreInvalidOperationException"></exception>
        private void RegisterReadWriteConfigure(IServiceCollection services)
        {
                services.AddSingleton<IReadWriteOptions<TShardingDbContext>, ReadWriteOptions<TShardingDbContext>>(sp =>
                    new ReadWriteOptions<TShardingDbContext>(
                        _shardingCoreConfigBuilder.ShardingConfigOption.ReadWriteDefaultPriority,
                        _shardingCoreConfigBuilder.ShardingConfigOption.ReadWriteDefaultEnable,
                        _shardingCoreConfigBuilder.ShardingConfigOption.ReadStrategyEnum,
                        _shardingCoreConfigBuilder.ShardingConfigOption.ReadConnStringGetStrategy));

                services
                    .AddSingleton<IShardingConnectionStringResolver<TShardingDbContext>,
                        ReadWriteShardingConnectionStringResolver<TShardingDbContext>>(sp =>
                    {
                        var readWriteConnectorFactory = sp.GetRequiredService<IReadWriteConnectorFactory>();
                        var readConnStrings = _shardingCoreConfigBuilder.ShardingConfigOption.ReadConnStringConfigure(sp);
                        var readWriteLoopConnectors = readConnStrings.Select(o => readWriteConnectorFactory.CreateConnector<TShardingDbContext>(_shardingCoreConfigBuilder.ShardingConfigOption.ReadStrategyEnum,o.Key,o.Value));

                        return new ReadWriteShardingConnectionStringResolver<TShardingDbContext>(
                            readWriteLoopConnectors);
                    });

                services.TryAddSingleton<IShardingReadWriteManager, ShardingReadWriteManager>();
                services.AddSingleton<IShardingReadWriteAccessor, ShardingReadWriteAccessor<TShardingDbContext>>();

        }
    }
}