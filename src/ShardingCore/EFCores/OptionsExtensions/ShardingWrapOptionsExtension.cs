using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core;

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
    public class ShardingWrapOptionsExtension : IDbContextOptionsExtension
    {

        public ShardingWrapOptionsExtension()
        {
        }
        public void ApplyServices(IServiceCollection services)
        {
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
            public override string LogFragment => "ShardingWrapOptionsExtension";
        }
    }
#endif
#if EFCORE3 || EFCORE5

     public class ShardingWrapOptionsExtension: IDbContextOptionsExtension
    {
        public void ApplyServices(IServiceCollection services)
        {
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
            public override string LogFragment => "ShardingWrapOptionsExtension";
        }
    }

#endif
#if EFCORE2

    public class ShardingWrapOptionsExtension: IDbContextOptionsExtension
    {
        public bool ApplyServices(IServiceCollection services)
        {
            return false;
        }

        public long GetServiceProviderHashCode() => 0;

        public void Validate(IDbContextOptions options)
        {
        }

        public string LogFragment => "ShardingWrapOptionsExtension";
    }
#endif
}
