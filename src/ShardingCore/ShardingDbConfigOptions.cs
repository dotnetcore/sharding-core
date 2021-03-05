using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Extensions;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/3 16:15:11
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingDbConfigOptions
    {
        

        private readonly Dictionary<Type,Type> _virtualRoutes = new Dictionary<Type, Type>();
        public void AddShardingTableRoute<TRoute>() where TRoute : IVirtualRoute
        {
            var routeType = typeof(TRoute);
            //获取类型
            var shardingEntityType = routeType.GetGenericArguments()[0];
            if (shardingEntityType == null)
                throw new ArgumentException("add sharding table route type error not assignable from IVirtualRoute<>");
            if (!_virtualRoutes.ContainsKey(shardingEntityType))
            {
                _virtualRoutes.Add(shardingEntityType, routeType);
            }
        }

        public Type GetVirtualRoute(Type entityType)
        {
            if (!_virtualRoutes.ContainsKey(entityType))
                throw new ArgumentException("not found IVirtualRoute");
            return _virtualRoutes[entityType];
        }
    }
}
