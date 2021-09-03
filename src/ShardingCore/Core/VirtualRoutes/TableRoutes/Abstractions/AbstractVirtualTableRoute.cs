using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.QueryRouteManagers;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.PaginationConfigurations;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 18 December 2020 14:33:01
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractVirtualTableRoute<T, TKey> : IVirtualTableRoute<T> where T : class, IShardingTable
    {
        public virtual IPaginationConfiguration<T> CreatePaginationConfiguration()
        {
            return null;
        }

        public Type ShardingEntityType => typeof(T);
        /// <summary>
        /// 如何将分表字段转成对应的类型
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <returns></returns>

        protected abstract TKey ConvertToShardingKey(object shardingKey);
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
        /// <returns></returns>
        public abstract List<IPhysicTable> RouteWithPredicate(List<IPhysicTable> allPhysicTables, IQueryable queryable);
        
        /// <summary>
        /// 根据值路由
        /// </summary>
        /// <param name="allPhysicTables"></param>
        /// <param name="shardingKeyValue"></param>
        /// <returns></returns>
        public abstract IPhysicTable RouteWithValue(List<IPhysicTable> allPhysicTables, object shardingKeyValue);
        /// <summary>
        /// 返回数据库现有的尾巴
        /// </summary>
        /// <returns></returns>
        public abstract List<string> GetAllTails();
    }
}