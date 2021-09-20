using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;

namespace ShardingCore.DIExtensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/19 22:21:45
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingDatabaseOptions
    {
        private readonly ICollection<Type> _virtualDataSourceRoutes = new LinkedList<Type>();

        /// <summary>
        /// 添加分表路由
        /// </summary>
        /// <typeparam name="TRoute"></typeparam>
        public void AddShardingDatabaseRoute<TRoute>() where TRoute : IVirtualDataSourceRoute
        {
            var routeType = typeof(TRoute);
            _virtualDataSourceRoutes.Add(routeType);
        }

        public ICollection<Type> GetShardingDatabaseRoutes()
        {
            return _virtualDataSourceRoutes;
        }
    }
    public class ShardingTableOptions
    {
        private readonly ICollection<Type> _virtualTableRoutes = new LinkedList<Type>();

        /// <summary>
        /// 添加分表路由
        /// </summary>
        /// <typeparam name="TRoute"></typeparam>
        public void AddShardingTableRoute<TRoute>() where TRoute : IVirtualTableRoute
        {
            var routeType = typeof(TRoute);
            _virtualTableRoutes.Add(routeType);
        }

        public ICollection<Type> GetShardingTableRoutes()
        {
            return _virtualTableRoutes;
        }
    }
}
