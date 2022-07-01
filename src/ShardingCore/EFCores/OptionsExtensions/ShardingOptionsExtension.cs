using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core;
using ShardingCore.Core.RuntimeContexts;

namespace ShardingCore.EFCores.OptionsExtensions
{


    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/10/17 20:27:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */

#if EFCORE6
    public class ShardingOptionsExtension : IDbContextOptionsExtension
    {
        private readonly IShardingRuntimeContext _shardingRuntimeContext;

        public ShardingOptionsExtension(IShardingRuntimeContext shardingRuntimeContext)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
        }
        public void ApplyServices(IServiceCollection services)
        {
            services.AddSingleton<IShardingRuntimeContext>(sp => _shardingRuntimeContext);
        }

        public void Validate(IDbContextOptions options)
        {
        }


        public DbContextOptionsExtensionInfo Info => new ShardingWrapDbContextOptionsExtensionInfo(this);

        private class ShardingWrapDbContextOptionsExtensionInfo : DbContextOptionsExtensionInfo
        {
            public ShardingWrapDbContextOptionsExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
            {
            }

            public override int GetServiceProviderHashCode() => 0;

            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => true;

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
            }

            public override bool IsDatabaseProvider => false;
            public override string LogFragment => "ShardingOptionsExtension";
        }
    }
#endif
#if EFCORE3 || EFCORE5

     public class ShardingOptionsExtension: IDbContextOptionsExtension
    {
        private readonly IShardingRuntimeContext _shardingRuntimeContext;

        public ShardingOptionsExtension(IShardingRuntimeContext shardingRuntimeContext)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
        }
        public void ApplyServices(IServiceCollection services)
        {
            services.AddSingleton<IShardingRuntimeContext>(sp => _shardingRuntimeContext);
        }

        public void Validate(IDbContextOptions options)
        {
        }


        public DbContextOptionsExtensionInfo Info => new ShardingWrapDbContextOptionsExtensionInfo(this);

        private class ShardingWrapDbContextOptionsExtensionInfo : DbContextOptionsExtensionInfo
        {
            public ShardingWrapDbContextOptionsExtensionInfo(IDbContextOptionsExtension extension) : base(extension) { }

            public override long GetServiceProviderHashCode() => 0;

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) { }

            public override bool IsDatabaseProvider => false;
            public override string LogFragment => "ShardingOptionsExtension";
        }
    }

#endif
#if EFCORE2

    public class ShardingOptionsExtension: IDbContextOptionsExtension
    {
        private readonly IShardingRuntimeContext _shardingRuntimeContext;

        public ShardingOptionsExtension(IShardingRuntimeContext shardingRuntimeContext)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
        }
        public bool ApplyServices(IServiceCollection services)
        {
            services.AddSingleton<IShardingRuntimeContext>(sp => _shardingRuntimeContext);
            return true;
        }
        public long GetServiceProviderHashCode() => 0;

        public void Validate(IDbContextOptions options)
        {
        }

        public string LogFragment => "ShardingOptionsExtension";
    }
#endif
}
