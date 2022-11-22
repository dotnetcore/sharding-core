#if EFCORE7

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ShardingCore.EFCores
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/10/17 20:27:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class UnionAllMergeOptionsExtension : IDbContextOptionsExtension
    {
        public void ApplyServices(IServiceCollection services)
        {
        }

        public void Validate(IDbContextOptions options)
        {
        }


        public DbContextOptionsExtensionInfo Info => new UnionAllMergeDbContextOptionsExtensionInfo(this);

        private class UnionAllMergeDbContextOptionsExtensionInfo : DbContextOptionsExtensionInfo
        {
            public UnionAllMergeDbContextOptionsExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
            {
            }

            public override int GetServiceProviderHashCode() => 0;

            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => true;

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
            }

            public override bool IsDatabaseProvider => false;
            public override string LogFragment => "UnionAllMergeOptionsExtension";
        }
    }
}

#endif