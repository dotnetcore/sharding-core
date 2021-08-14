using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Utils;

namespace ShardingCore.Core.VirtualTables
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 14:20:12
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 同数据库虚拟表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OneDbVirtualTable<T> : IVirtualTable<T> where T : class, IShardingTable
    {
        private readonly IVirtualTableRoute<T> _virtualTableRoute;

        public Type EntityType => typeof(T);
        public ShardingTableConfig ShardingConfig { get; }
        private readonly List<IPhysicTable> _physicTables = new List<IPhysicTable>();

        public OneDbVirtualTable(IVirtualTableRoute<T> virtualTableRoute)
        {
            _virtualTableRoute = virtualTableRoute;
            ShardingConfig = ShardingKeyUtil.Parse(EntityType);
        }

        public List<IPhysicTable> GetAllPhysicTables()
        {
            return _physicTables;
        }

        public List<IPhysicTable> RouteTo(TableRouteConfig tableRouteConfig)
        {
            var route = _virtualTableRoute;
            if (tableRouteConfig.UseQueryable())
                return route.RouteWithWhere(_physicTables, tableRouteConfig.GetQueryable());
            if (tableRouteConfig.UsePredicate())
                return route.RouteWithWhere(_physicTables, new EnumerableQuery<T>((Expression<Func<T, bool>>) tableRouteConfig.GetPredicate()));
            object shardingKeyValue = null;
            if (tableRouteConfig.UseValue())
                shardingKeyValue = tableRouteConfig.GetShardingKeyValue();

            if (tableRouteConfig.UseEntity())
                shardingKeyValue = tableRouteConfig.GetShardingEntity().GetPropertyValue(ShardingConfig.ShardingField);

            if (shardingKeyValue != null)
            {
                var routeWithValue = route.RouteWithValue(_physicTables, shardingKeyValue);
                return new List<IPhysicTable>(1) {routeWithValue};
            }

            throw new NotImplementedException(nameof(TableRouteConfig));
        }


        public void AddPhysicTable(IPhysicTable physicTable)
        {
            if (_physicTables.All(o => o.Tail != physicTable.Tail))
                _physicTables.Add(physicTable);
        }

        public void SetOriginalTableName(string originalTableName)
        {
            ShardingConfig.ShardingOriginalTable = originalTableName;
        }

        public string GetOriginalTableName()
        {
            return ShardingConfig.ShardingOriginalTable;
        }

        IVirtualTableRoute IVirtualTable.GetVirtualRoute()
        {
            return GetVirtualRoute();
        }

        public List<string> GetTaleAllTails()
        {
            return _physicTables.Select(o => o.Tail).ToList();
        }

        public IVirtualTableRoute<T> GetVirtualRoute()
        {
            return _virtualTableRoute;
        }
    }
}