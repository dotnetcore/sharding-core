using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShardingCore.Core.VirtualRoutes.TableRoutes;

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
        public void AddShardingTableRoute<TRoute>() where TRoute : IVirtualTableRoute
        {
            var routeType = typeof(TRoute);
            //获取类型
            var genericVirtualRoute = routeType.GetInterfaces().FirstOrDefault(it => it.IsInterface && it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IVirtualTableRoute<>)
                && it.GetGenericArguments().Any());
            if (genericVirtualRoute == null)
                throw new ArgumentException("add sharding route type error not assignable from IVirtualTableRoute<>.");

            var shardingEntityType = genericVirtualRoute.GetGenericArguments()[0];
            if (shardingEntityType == null)
                throw new ArgumentException("add sharding table route type error not assignable from IVirtualTableRoute<>");
            if (!_virtualRoutes.ContainsKey(shardingEntityType))
            {
                _virtualRoutes.Add(shardingEntityType, routeType);
            }
        }

        public Type GetVirtualRoute(Type entityType)
        {
            if (!_virtualRoutes.ContainsKey(entityType))
                throw new ArgumentException("not found IVirtualTableRoute");
            return _virtualRoutes[entityType];
        }
    }
}
