using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualRoutes;
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
    public class OneDbVirtualTable<T> : IVirtualTable<T> where T : class, IShardingEntity
    {
        private readonly IVirtualRoute<T> _virtualRoute;

        public Type EntityType => typeof(T);
        public ShardingEntityConfig ShardingConfig { get; }
        private readonly List<IPhysicTable> _physicTables = new List<IPhysicTable>();

        public OneDbVirtualTable(IServiceProvider serviceProvider)
        {
            _virtualRoute = serviceProvider.GetService<IVirtualRoute<T>>() ?? throw new ShardingOwnerNotFoundException($"{EntityType}");
            ShardingConfig = ShardingKeyUtil.Parse(EntityType);
        }

        public List<IPhysicTable> GetAllPhysicTables()
        {
            return _physicTables;
        }

        public List<IPhysicTable> RouteTo(RouteConfig routeConfig)
        {
            var route = _virtualRoute;
            if (routeConfig.UseQueryable())
                return route.RouteWithWhere(_physicTables, routeConfig.GetQueryable());
            if (routeConfig.UsePredicate())
                return route.RouteWithWhere(_physicTables, new EnumerableQuery<T>((Expression<Func<T, bool>>) routeConfig.GetPredicate()));
            object shardingKeyValue = null;
            if (routeConfig.UseValue())
                shardingKeyValue = routeConfig.GetShardingKeyValue();

            if (routeConfig.UseEntity())
                shardingKeyValue = routeConfig.GetShardingEntity().GetPropertyValue(ShardingConfig.ShardingField);

            if (shardingKeyValue != null)
            {
                var routeWithValue = route.RouteWithValue(_physicTables, shardingKeyValue);
                return new List<IPhysicTable>(1) {routeWithValue};
            }

            throw new NotImplementedException(nameof(routeConfig));
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

        IVirtualRoute IVirtualTable.GetVirtualRoute()
        {
            return GetVirtualRoute();
        }

        public List<string> GetTaleAllTails()
        {
            return _virtualRoute.GetAllTails();
        }

        public IVirtualRoute<T> GetVirtualRoute()
        {
            return _virtualRoute;
        }
    }
}