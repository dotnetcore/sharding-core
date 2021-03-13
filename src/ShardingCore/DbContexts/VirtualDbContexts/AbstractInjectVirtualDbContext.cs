using System;
using Microsoft.Extensions.DependencyInjection;

namespace ShardingCore.DbContexts.VirtualDbContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 01 January 2021 16:35:19
* @Email: 326308290@qq.com
*/
    public abstract class AbstractInjectVirtualDbContext
    {
        public IServiceProvider ServiceProvider { get; }

        public AbstractInjectVirtualDbContext(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
        protected TService GetService<TService>() => ServiceProvider.GetService<TService>();

        protected TService LazyGet<TService>(ref TService reference)
            => LazyTypeGet(typeof(TService), ref reference);

        protected TRef LazyTypeGet<TRef>(Type serviceType, ref TRef reference)
        {
            return reference ??= (TRef)ServiceProvider.GetService(serviceType);
        }

    }
}