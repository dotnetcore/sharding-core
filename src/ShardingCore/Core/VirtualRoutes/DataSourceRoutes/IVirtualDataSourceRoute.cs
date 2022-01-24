using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.EntityShardingMetadatas;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Sharding.EntityQueryConfigurations;
using ShardingCore.Sharding.PaginationConfigurations;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 05 February 2021 13:03:58
* @Email: 326308290@qq.com
*/
    public interface IVirtualDataSourceRoute
    {
        EntityMetadata EntityMetadata { get; }
        /// <summary>
        /// 分页配置
        /// </summary>
        PaginationMetadata PaginationMetadata { get; }
        /// <summary>
        /// 是否启用分页配置
        /// </summary>
        bool EnablePagination { get; }
        string ShardingKeyToDataSourceName(object shardingKeyValue);

        /// <summary>
        /// 根据查询条件路由返回物理数据源
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="isQuery"></param>
        /// <returns>data source name</returns>
        List<string> RouteWithPredicate(IQueryable queryable, bool isQuery);

        /// <summary>
        /// 根据值进行路由
        /// </summary>
        /// <param name="shardingKeyValue"></param>
        /// <returns>data source name</returns>
        string RouteWithValue(object shardingKeyValue);

        List<string> GetAllDataSourceNames();
        /// <summary>
        /// 添加数据源
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        bool AddDataSourceName(string dataSourceName);

    }
    
    public interface IVirtualDataSourceRoute<TEntity> : IVirtualDataSourceRoute, IEntityMetadataDataSourceConfiguration<TEntity> where TEntity : class
    {
        /// <summary>
        /// 返回null就是表示不开启分页配置
        /// </summary>
        /// <returns></returns>
        IPaginationConfiguration<TEntity> CreatePaginationConfiguration();
        ///// <summary>
        ///// 配置查询
        ///// </summary>
        ///// <returns></returns>
        //IEntityQueryConfiguration<TEntity> CreateEntityQueryConfiguration();
    }
}