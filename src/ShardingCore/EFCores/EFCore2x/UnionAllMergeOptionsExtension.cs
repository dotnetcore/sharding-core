#if EFCORE2
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
    public class UnionAllMergeOptionsExtension: IDbContextOptionsExtension
    {
        public bool ApplyServices(IServiceCollection services)
        {
            return false;
        }

        public long GetServiceProviderHashCode() => 0;

        public void Validate(IDbContextOptions options)
        {
        }

        public string LogFragment => "UnionAllMergeOptionsExtension";
    }
}
#endif
