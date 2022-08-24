using System;

namespace ShardingCore.Core.ServiceProviders
{

    /// <summary>
    /// 默认的分片服务提供者
    /// </summary>
    public interface IShardingProvider
    {
     /// <summary>
     /// 优先通过ShardingCore的IServiceProvider获取
     /// 没有再通过ApplicationServiceProvider获取
     /// </summary>
     /// <param name="serviceType"></param>
     /// <param name="tryApplicationServiceProvider">是否尝试通过ApplicationServiceProvider获取</param>
     /// <returns></returns>
        object GetService(Type serviceType,bool tryApplicationServiceProvider=true);
    
        /// <summary>
        /// 优先通过ShardingCore的IServiceProvider获取
        /// 没有再通过ApplicationServiceProvider获取
        /// </summary>
        /// <param name="tryApplicationServiceProvider">是否尝试通过ApplicationServiceProvider获取</param>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService GetService<TService>(bool tryApplicationServiceProvider=true);
        
        /// <summary>
        /// 优先通过ShardingCore的IServiceProvider获取
        /// 没有再通过ApplicationServiceProvider获取
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="tryApplicationServiceProvider">是否尝试通过ApplicationServiceProvider获取</param>
        /// <returns></returns>
         object GetRequiredService(Type serviceType,bool tryApplicationServiceProvider=true);
        /// <summary>
        /// 
        /// 优先通过ShardingCore的IServiceProvider获取
        /// 没有再通过ApplicationServiceProvider获取
        /// </summary>
        /// <param name="tryApplicationServiceProvider">是否尝试通过ApplicationServiceProvider获取</param>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
         TService GetRequiredService<TService>(bool tryApplicationServiceProvider=true);
        
        IServiceProvider ApplicationServiceProvider { get; }

        IShardingScope CreateScope();
    }
}