using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/17 14:22:10
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal abstract class AbstractInMemoryAsyncMergeEngine<TEntity> : AbstractBaseMergeEngine<TEntity>, IInMemoryAsyncMergeEngine<TEntity>
    {
        private readonly StreamMergeContext<TEntity> _mergeContext;

        public AbstractInMemoryAsyncMergeEngine(StreamMergeContext<TEntity> streamMergeContext)
        {
            _mergeContext = streamMergeContext;
        }

        private (IQueryable queryable, DbContext dbContext) CreateAsyncExecuteQueryable<TResult>(string dsname, TableRouteResult tableRouteResult, ConnectionModeEnum connectionMode)
        {
            var shardingDbContext = _mergeContext.CreateDbContext(dsname, tableRouteResult, connectionMode);
            var newQueryable = (IQueryable<TEntity>)GetStreamMergeContext().GetReWriteQueryable()
                .ReplaceDbContextQueryable(shardingDbContext);
            return (newQueryable, shardingDbContext);
            ;
        }

        public async Task<List<RouteQueryResult<TResult>>> ExecuteAsync<TResult>(Func<IQueryable, Task<TResult>> efQuery, CancellationToken cancellationToken = new CancellationToken())
        {
            var routeQueryResults = _mergeContext.PreperExecute(() => new List<RouteQueryResult<TResult>>(0));
            if (routeQueryResults != null)
                return routeQueryResults;
            var defaultSqlRouteUnits = GetDefaultSqlRouteUnits();
            var waitExecuteQueue = GetDataSourceGroupAndExecutorGroup<RouteQueryResult<TResult>>(true, defaultSqlRouteUnits,
                   async sqlExecutorUnit =>
                    {
                        var connectionMode = _mergeContext.RealConnectionMode(sqlExecutorUnit.ConnectionMode);
                        var dataSourceName = sqlExecutorUnit.RouteUnit.DataSourceName;
                        var routeResult = sqlExecutorUnit.RouteUnit.TableRouteResult;

                        var (asyncExecuteQueryable, dbContext) =
                            CreateAsyncExecuteQueryable<TResult>(dataSourceName, routeResult, connectionMode);

                        var queryResult = await efQuery(asyncExecuteQueryable);
                        var routeQueryResult = new RouteQueryResult<TResult>(dataSourceName, routeResult, queryResult);
                        return new ShardingMergeResult<RouteQueryResult<TResult>>(dbContext, routeQueryResult);
                    }).ToArray();

            return (await Task.WhenAll(waitExecuteQueue)).SelectMany(o => o).ToList();
        }


        ///// <summary>
        ///// 异步并发查询
        ///// </summary>
        ///// <typeparam name="TResult"></typeparam>
        ///// <param name="queryable"></param>
        ///// <param name="dataSourceName"></param>
        ///// <param name="routeResult"></param>
        ///// <param name="efQuery"></param>
        ///// <param name="cancellationToken"></param>
        ///// <returns></returns>
        //public async Task<RouteQueryResult<TResult>> AsyncParallelResultExecute<TResult>(IQueryable queryable,string dataSourceName,TableRouteResult routeResult, Func<IQueryable, Task<TResult>> efQuery,
        //    CancellationToken cancellationToken = new CancellationToken())
        //{
        //    cancellationToken.ThrowIfCancellationRequested();
        //    var queryResult = await efQuery(queryable);

        //    return new RouteQueryResult<TResult>(dataSourceName, routeResult, queryResult);
        //}

        protected override StreamMergeContext<TEntity> GetStreamMergeContext()
        {
            return _mergeContext;
        }
    }
}