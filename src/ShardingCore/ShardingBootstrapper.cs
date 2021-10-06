using ShardingCore.Extensions;
using System;
using System.Collections.Generic;

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

        public ShardingBootstrapper(IServiceProvider serviceProvider)
        {
            ShardingContainer.SetServices(serviceProvider);
            _shardingConfigOptions = ShardingContainer.GetServices<IShardingConfigOption>();
        }

        public void Start()
        {
            foreach (var shardingConfigOption in _shardingConfigOptions)
            {
                var instance = (IShardingDbContextBootstrapper)Activator.CreateInstance(typeof(ShardingDbContextBootstrapper<>).GetGenericType0(shardingConfigOption.ShardingDbContextType), shardingConfigOption);
                instance.Init();
            }
        }

    }
}