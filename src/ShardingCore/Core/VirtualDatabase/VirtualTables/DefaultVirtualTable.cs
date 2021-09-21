using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core.VirtualDatabase;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;

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
    public class DefaultVirtualTable<T> : IVirtualTable<T> where T : class,IShardingTable
    {
        private readonly IVirtualTableRoute<T> _virtualTableRoute;
          
        /// <summary>
        /// 分表的对象类型
        /// </summary>
        public Type EntityType { get; }
        /// <summary>
        /// 分表的配置
        /// </summary>
        public ShardingEntityConfig ShardingConfig { get; }
        /// <summary>
        /// 分库配置
        /// </summary>
        public PaginationMetadata PaginationMetadata { get; }
        /// <summary>
        /// 是否启用智能分页
        /// </summary>
        public bool EnablePagination => PaginationMetadata != null;


        private readonly ConcurrentDictionary<IPhysicTable, object> _physicTables = new ConcurrentDictionary<IPhysicTable, object>();

        public DefaultVirtualTable(IVirtualTableRoute<T> virtualTableRoute)
        {
               _virtualTableRoute = virtualTableRoute;
            EntityType = typeof(T);
            ShardingConfig = ShardingUtil.Parse(EntityType);
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

        public List<IPhysicTable> RouteTo(ShardingTableRouteConfig tableRouteConfig)
        {
            var route = _virtualTableRoute;
            if (tableRouteConfig.UseQueryable())
                return route.RouteWithPredicate(GetAllPhysicTables(), tableRouteConfig.GetQueryable(), true);
            if (tableRouteConfig.UsePredicate())
                return route.RouteWithPredicate(GetAllPhysicTables(), new EnumerableQuery<T>((Expression<Func<T, bool>>)tableRouteConfig.GetPredicate()), false);
            object shardingKeyValue = null;
            if (tableRouteConfig.UseValue())
                shardingKeyValue = tableRouteConfig.GetShardingKeyValue();

            if (tableRouteConfig.UseEntity())
                shardingKeyValue = tableRouteConfig.GetShardingEntity().GetPropertyValue(ShardingConfig.ShardingTableField);

            if (shardingKeyValue == null)
                throw new ShardingCoreException(" route entity queryable or sharding key value is null ");
            var routeWithValue = route.RouteWithValue(GetAllPhysicTables(), shardingKeyValue);
            return new List<IPhysicTable>(1) { routeWithValue };
        }


        public bool AddPhysicTable(IPhysicTable physicTable)
        {
            if (physicTable.EntityType != EntityType)
                throw new InvalidOperationException($"virtual table entity type :[{EntityType.FullName}] physic table entity type:[{physicTable.EntityType.FullName}]");
            return _physicTables.TryAdd(physicTable, null);
        }

        public void SetVirtualTableName(string originalTableName)
        {
            ShardingConfig.VirtualTableName = originalTableName;
        }

        public string GetVirtualTableName()
        {
            return ShardingConfig.VirtualTableName;
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