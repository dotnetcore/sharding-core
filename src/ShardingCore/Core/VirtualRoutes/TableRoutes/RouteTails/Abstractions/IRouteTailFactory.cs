using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 22 August 2021 14:58:19
* @Email: 326308290@qq.com
*/
    public interface IRouteTailFactory
    {
        /// <summary>
        /// dbcontext模型会被缓存
        /// </summary>
        /// <param name="tail"></param>
        /// <returns></returns>
        IRouteTail Create(string tail);
        /// <summary>
        /// cache false创建的dbcontext模型不会被缓存
        /// </summary>
        /// <param name="tail"></param>
        /// <param name="cache"></param>
        /// <returns></returns>
        IRouteTail Create(string tail, bool cache);
        /// <summary>
        /// dbcontext模型不会被缓存
        /// </summary>
        /// <param name="tableRouteResult"></param>
        /// <returns></returns>
        IRouteTail Create(TableRouteResult tableRouteResult);
    }
}