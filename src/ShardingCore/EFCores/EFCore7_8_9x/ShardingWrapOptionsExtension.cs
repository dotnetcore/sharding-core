﻿#if EFCORE7 || EFCORE8 || EFCORE9

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core;

namespace ShardingCore.EFCores
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/10/17 20:27:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
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
}

#endif