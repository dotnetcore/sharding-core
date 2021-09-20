using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.TableCreator;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 21 December 2020 09:10:07
    * @Email: 326308290@qq.com
    */
    public class ShardingBootstrapper : IShardingBootstrapper
    {
        private readonly IEnumerable<IShardingConfigOption> _shardingConfigOptions;

        public ShardingBootstrapper(IServiceProvider serviceProvider, IEnumerable<IShardingConfigOption> shardingConfigOptions)
        {
            ShardingContainer.SetServices(serviceProvider);
            _shardingConfigOptions = shardingConfigOptions;
        }

        public void Start()
        {
            foreach (var shardingConfigOption in _shardingConfigOptions)
            {
                var instance = (IShardingDbContextBootstrapper)Activator.CreateInstance(typeof(ShardingDbContextBootstrapper<>).GetGenericType0(shardingConfigOption.ShardingDbContextType), shardingConfigOption);
                instance.Initialize();
            }
        }

    }
}