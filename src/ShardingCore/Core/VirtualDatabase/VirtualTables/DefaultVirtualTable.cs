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
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ShardingEnumerableQueries;
using ShardingCore.Core.VirtualDatabase;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Sharding.EntityQueryConfigurations;

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
    public class DefaultVirtualTable<T> : IVirtualTable<T> where T : class
    {
        public EntityMetadata EntityMetadata { get; }
        private readonly IVirtualTableRoute<T> _virtualTableRoute;
          
        /// <summary>
        /// 分页配置
        /// </summary>
        public PaginationMetadata PaginationMetadata { get; }

        /// <summary>
        /// 是否启用智能分页
        /// </summary>
        public bool EnablePagination => PaginationMetadata != null;
        /// <summary>
        /// 查询配置
        /// </summary>
        public EntityQueryMetadata EntityQueryMetadata { get; }
        /// <summary>
        /// 是否启用表达式分片配置
        /// </summary>
        public bool EnableEntityQuery => EntityQueryMetadata != null;


        private readonly ConcurrentDictionary<IPhysicTable, object> _physicTables = new ConcurrentDictionary<IPhysicTable, object>();

        public DefaultVirtualTable(IVirtualTableRoute<T> virtualTableRoute,EntityMetadata entityMetadata)
        {
            EntityMetadata = entityMetadata;
            _virtualTableRoute = virtualTableRoute;
            var paginationConfiguration = virtualTableRoute.CreatePaginationConfiguration();
            if (paginationConfiguration!=null)
            {
                PaginationMetadata = new PaginationMetadata();
                var paginationBuilder = new PaginationBuilder<T>(PaginationMetadata);
                paginationConfiguration.Configure(paginationBuilder);
            }

            var entityQueryConfiguration = virtualTableRoute.CreateEntityQueryConfiguration();
            if (entityQueryConfiguration != null)
            {
                EntityQueryMetadata = new EntityQueryMetadata();
                var entityQueryBuilder = new EntityQueryBuilder<T>(EntityQueryMetadata);
                entityQueryConfiguration.Configure(entityQueryBuilder);
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
                return route.RouteWithPredicate(GetAllPhysicTables(), new ShardingEmptyEnumerableQuery<T>((Expression<Func<T, bool>>)tableRouteConfig.GetPredicate()).EmptyQueryable(), false);
            object shardingKeyValue = null;
            if (tableRouteConfig.UseValue())
                shardingKeyValue = tableRouteConfig.GetShardingKeyValue();

            if (tableRouteConfig.UseEntity())
                shardingKeyValue = tableRouteConfig.GetShardingEntity().GetPropertyValue(EntityMetadata.ShardingTableProperty.Name);

            if (shardingKeyValue == null)
                throw new ShardingCoreException(" route entity queryable or sharding key value is null ");
            var routeWithValue = route.RouteWithValue(GetAllPhysicTables(), shardingKeyValue);
            return new List<IPhysicTable>(1) { routeWithValue };
        }


        public bool AddPhysicTable(IPhysicTable physicTable)
        {
            if (physicTable.EntityType != EntityMetadata.EntityType)
                throw new ShardingCoreInvalidOperationException($"virtual table entity type :[{EntityMetadata.EntityType.FullName}] physic table entity type:[{physicTable.EntityType.FullName}]");
            return _physicTables.TryAdd(physicTable, null);
        }

        public string GetVirtualTableName()
        {
            return EntityMetadata.VirtualTableName;
        }

        IVirtualTableRoute IVirtualTable.GetVirtualRoute()
        {
            return GetVirtualRoute();
        }

        public List<string> GetTableAllTails()
        {
            return _physicTables.Keys.Select(o => o.Tail).ToList();
        }

        public IVirtualTableRoute<T> GetVirtualRoute()
        {
            return _virtualTableRoute;
        }
    }
}