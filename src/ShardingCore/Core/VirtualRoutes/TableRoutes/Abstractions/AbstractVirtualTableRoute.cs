using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.QueryRouteManagers;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.MergeEngines.ParallelControl;
using ShardingCore.Sharding.PaginationConfigurations;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 18 December 2020 14:33:01
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractVirtualTableRoute<T, TKey> : IVirtualTableRoute<T>, IEntityMetadataAutoBindInitializer where T : class
    {

        private readonly DoOnlyOnce _doOnlyOnce = new DoOnlyOnce();
        public void Initialize(EntityMetadata entityMetadata)
        {
            if (!_doOnlyOnce.IsUnDo())
                throw new ShardingCoreInvalidOperationException("already init");
            EntityMetadata = entityMetadata;
        }
        public virtual IPaginationConfiguration<T> CreatePaginationConfiguration()
        {
            return null;
        }

        public EntityMetadata EntityMetadata { get; private set; }

        /// <summary>
        /// 如何将分表字段转成后缀
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        public abstract string ShardingKeyToTail(object shardingKey);

        /// <summary>
        /// 根据表达式路由
        /// </summary>
        /// <param name="allPhysicTables"></param>
        /// <param name="queryable"></param>
        /// <param name="isQuery"></param>
        /// <returns></returns>
        public abstract List<IPhysicTable> RouteWithPredicate(List<IPhysicTable> allPhysicTables, IQueryable queryable,bool isQuery);
        
        /// <summary>
        /// 根据值路由
        /// </summary>
        /// <param name="allPhysicTables"></param>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        public abstract IPhysicTable RouteWithValue(List<IPhysicTable> allPhysicTables, object shardingKey);
        /// <summary>
        /// 返回数据库现有的尾巴
        /// </summary>
        /// <returns></returns>
        public abstract List<string> GetAllTails();

        /// <summary>
        /// 配置分表信息
        /// </summary>
        /// <param name="builder"></param>
        public abstract void Configure(EntityMetadataTableBuilder<T> builder);
    }
}