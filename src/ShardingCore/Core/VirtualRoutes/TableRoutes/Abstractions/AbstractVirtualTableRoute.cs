using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.MergeEngines.ParallelControl;
using ShardingCore.Sharding.PaginationConfigurations;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Sharding.EntityQueryConfigurations;

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
        public IShardingEntityConfigOptions EntityConfigOptions { get; private set; }
        public virtual void Initialize(EntityMetadata entityMetadata)
        {
            if (!_doOnlyOnce.IsUnDo())
                throw new ShardingCoreInvalidOperationException("already init");
            EntityMetadata = entityMetadata;

            EntityConfigOptions =
                ShardingContainer.GetRequiredShardingEntityConfigOption(entityMetadata.ShardingDbContextType);
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
        /// 配置分表的一些信息
        /// 1.ShardingProperty 哪个字段分表
        /// 2.TableSeparator 分表的后缀和表名的连接符
        /// 3.AutoCreateTable 启动时是否需要创建对应的分表信息
        /// </summary>
        /// <param name="builder"></param>
        public abstract void Configure(EntityMetadataTableBuilder<T> builder);

        public virtual IPaginationConfiguration<T> CreatePaginationConfiguration()
        {
            return null;
        }
        public virtual IEntityQueryConfiguration<T> CreateEntityQueryConfiguration()
        {
            return null;
        }
    }
}