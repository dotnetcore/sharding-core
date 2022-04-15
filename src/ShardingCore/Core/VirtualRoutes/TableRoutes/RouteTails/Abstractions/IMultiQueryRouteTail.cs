using System;
using System.Collections.Generic;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Sunday, 22 August 2021 09:44:54
    * @Email: 326308290@qq.com
    */
    public interface IMultiQueryRouteTail: IRouteTail, INoCacheRouteTail
    {
        /// <summary>
        /// 获取对象类型的应该后缀
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        string GetEntityTail(Type entityType);

        ISet<Type> GetEntityTypes();
    }
}