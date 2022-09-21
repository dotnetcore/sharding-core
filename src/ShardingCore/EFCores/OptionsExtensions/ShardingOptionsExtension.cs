using System;
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
#if !EFCORE2 && !EFCORE3 && !EFCORE5 && !EFCORE6
    error
#endif

#if EFCORE6
    public class ShardingOptionsExtension : IDbContextOptionsExtension
    {
        public IShardingRuntimeContext ShardingRuntimeContext { get; }

        public ShardingOptionsExtension(IShardingRuntimeContext shardingRuntimeContext)
        {
            ShardingRuntimeContext = shardingRuntimeContext;
        }

        public void ApplyServices(IServiceCollection services)
        {
            services.AddSingleton<IShardingRuntimeContext>(sp => ShardingRuntimeContext);
        }

        public void Validate(IDbContextOptions options)
        {
        }


        public DbContextOptionsExtensionInfo Info => new ShardingOptionsExtensionInfo(this);

        private class ShardingOptionsExtensionInfo : DbContextOptionsExtensionInfo
        {
            private readonly ShardingOptionsExtension _shardingOptionsExtension;

            public ShardingOptionsExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
            {
                _shardingOptionsExtension = (ShardingOptionsExtension)extension;
            }

            public override int GetServiceProviderHashCode() =>
                _shardingOptionsExtension.ShardingRuntimeContext.GetHashCode();

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
        public IShardingRuntimeContext ShardingRuntimeContext { get; }

        public ShardingOptionsExtension(IShardingRuntimeContext shardingRuntimeContext)
        {
            ShardingRuntimeContext = shardingRuntimeContext;
        }
        public void ApplyServices(IServiceCollection services)
        {
            services.AddSingleton<IShardingRuntimeContext>(sp => ShardingRuntimeContext);
        }

        public void Validate(IDbContextOptions options)
        {
        }


        public DbContextOptionsExtensionInfo Info => new ShardingOptionsExtensionInfo(this);

        private class ShardingOptionsExtensionInfo : DbContextOptionsExtensionInfo
        {
            private readonly ShardingOptionsExtension _shardingOptionsExtension;
            public ShardingOptionsExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
            {
                _shardingOptionsExtension = (ShardingOptionsExtension)extension;
            }


            public override long GetServiceProviderHashCode() => _shardingOptionsExtension.ShardingRuntimeContext.GetHashCode();

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) { }

            public override bool IsDatabaseProvider => false;
            public override string LogFragment => "ShardingOptionsExtension";
        }
    }

#endif
#if EFCORE2
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
#endif
}