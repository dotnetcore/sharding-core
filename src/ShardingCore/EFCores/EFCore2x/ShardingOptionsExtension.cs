#if EFCORE2
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core;
using ShardingCore.Core.RuntimeContexts;

namespace ShardingCore.EFCores
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/10/17 20:27:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingOptionsExtension: IDbContextOptionsExtension
    {
        public IShardingRuntimeContext ShardingRuntimeContext { get; }

        public ShardingOptionsExtension(IShardingRuntimeContext shardingRuntimeContext)
        {
            ShardingRuntimeContext = shardingRuntimeContext;
        }
        public bool ApplyServices(IServiceCollection services)
        {
            services.AddSingleton<IShardingRuntimeContext>(sp => ShardingRuntimeContext);
            return true;
        }
        public long GetServiceProviderHashCode() => ShardingRuntimeContext.GetHashCode();

        public void Validate(IDbContextOptions options)
        {
        }

        public string LogFragment => "ShardingOptionsExtension";
    }
}
#endif