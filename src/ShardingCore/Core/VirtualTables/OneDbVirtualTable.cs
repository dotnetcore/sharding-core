using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.PaginationConfigurations;
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

        public PaginationMetadata PaginationMetadata { get; }
        public bool EnablePagination => PaginationMetadata != null;

        private readonly ConcurrentDictionary<IPhysicTable,object> _physicTables = new ConcurrentDictionary<IPhysicTable,object>();

        public OneDbVirtualTable(IVirtualTableRoute<T> virtualTableRoute)
        {
            _virtualTableRoute = virtualTableRoute;
            ShardingConfig = ShardingKeyUtil.Parse(EntityType);
            var paginationConfiguration = virtualTableRoute.CreatePaginationConfiguration();
            if (paginationConfiguration != null)
            {
                PaginationMetadata = new PaginationMetadata();
                var paginationBuilder = new PaginationBuilder<T>(PaginationMetadata);
                paginationConfiguration.Configure(paginationBuilder);
            }
        }

        public List<IPhysicTable> GetAllPhysicTables()
        {
            return _physicTables.Keys.ToList();
        }

        public List<IPhysicTable> RouteTo(TableRouteConfig tableRouteConfig)
        {
            var route = _virtualTableRoute;
            if (tableRouteConfig.UseQueryable())
                return route.RouteWithPredicate(GetAllPhysicTables(), tableRouteConfig.GetQueryable(),true);
            if (tableRouteConfig.UsePredicate())
                return route.RouteWithPredicate(GetAllPhysicTables(), new EnumerableQuery<T>((Expression<Func<T, bool>>) tableRouteConfig.GetPredicate()),false);
            object shardingKeyValue = null;
            if (tableRouteConfig.UseValue())
                shardingKeyValue = tableRouteConfig.GetShardingKeyValue();

            if (tableRouteConfig.UseEntity())
                shardingKeyValue = tableRouteConfig.GetShardingEntity().GetPropertyValue(ShardingConfig.ShardingField);

            if (shardingKeyValue != null)
            {
                var routeWithValue = route.RouteWithValue(GetAllPhysicTables(), shardingKeyValue);
                return new List<IPhysicTable>(1) {routeWithValue};
            }

            throw new ShardingCoreException(" route entity queryable or sharding key value is null ");
        }


        public void AddPhysicTable(IPhysicTable physicTable)
        {
            if (GetAllPhysicTables().All(o => o.Tail != physicTable.Tail))
                _physicTables.TryAdd(physicTable,null);
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
            return _physicTables.Keys.Select(o => o.Tail).ToList();
        }

        public IVirtualTableRoute<T> GetVirtualRoute()
        {
            return _virtualTableRoute;
        }
    }
}