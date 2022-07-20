using System;
using System.Collections.Generic;

namespace ShardingCore.Core.RuntimeContexts
{
    public interface IShardingRuntimeContextManager
    {
        /// <summary>
        /// 尝试获取注册的dbcontext type对应的 没有返回null
        /// </summary>
        /// <param name="dbContextType"></param>
        /// <returns></returns>
        IShardingRuntimeContext TryGet(Type dbContextType);

        IReadOnlyDictionary<Type, IShardingRuntimeContext> GetAll();
    }
}