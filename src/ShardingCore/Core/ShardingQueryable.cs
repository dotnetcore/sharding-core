using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.Internal.StreamMerge;
using ShardingCore.Core.Internal.StreamMerge.ListMerge;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.Extensions;

#if EFCORE2
using Microsoft.EntityFrameworkCore.Extensions.Internal;
#endif

namespace ShardingCore.Core
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 22 December 2020 16:26:16
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 分表查询构造器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ShardingQueryable<T> : IShardingQueryable<T> 
    {
        private IQueryable<T> _source;
        private bool _autoParseRoute = true;
        private readonly IShardingScopeFactory _shardingScopeFactory;
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly IShardingParallelDbContextFactory _shardingParallelDbContextFactory;
        public Dictionary<Type, Expression> _routes = new Dictionary<Type, Expression>();
        private readonly Dictionary<IVirtualTable, List<string>> _endRoutes = new Dictionary<IVirtualTable, List<string>>();


        public ShardingQueryable(IQueryable<T> source)
        {
            _source = source;
            _shardingScopeFactory = ShardingContainer.Services.GetService<IShardingScopeFactory>();
            _virtualTableManager = ShardingContainer.Services.GetService<IVirtualTableManager>();
            _shardingParallelDbContextFactory = ShardingContainer.Services.GetService<IShardingParallelDbContextFactory>();
        }


        public IShardingQueryable<T> EnableAutoRouteParse()
        {
            _autoParseRoute = true;
            return this;
        }

        public IShardingQueryable<T> DisableAutoRouteParse()
        {
            _autoParseRoute = false;
            return this;
        }

        public IShardingQueryable<T> AddManualRoute<TShardingEntity>(Expression<Func<TShardingEntity, bool>> predicate) where TShardingEntity : class, IShardingEntity
        {
            var shardingEntityType = typeof(TShardingEntity);
            if (!_routes.ContainsKey(shardingEntityType))
            {
                ((Expression<Func<TShardingEntity, bool>>) _routes[shardingEntityType]).And(predicate);
            }
            else
            {
                _routes.Add(typeof(TShardingEntity), predicate);
            }

            return this;
        }

        public IShardingQueryable<T> AddManualRoute(IVirtualTable virtualTable, string tail)
        {
            if (_endRoutes.ContainsKey(virtualTable))
            {
                var tails = _endRoutes[virtualTable];
                if (!tails.Contains(tail))
                {
                    tails.Add(tail);
                }
            }
            else
            {
                _endRoutes.Add(virtualTable, new List<string>()
                {
                    tail
                });
            }

            return this;
        }

        public async Task<int> CountAsync()
        {
            var shardingCounts = await GetShardingQueryAsync(async queryable =>await EntityFrameworkQueryableExtensions.CountAsync((IQueryable<T>) queryable));
            return shardingCounts.Sum();
        }

        public async Task<long> LongCountAsync()
        {
            var shardingCounts = await GetShardingQueryAsync(async queryable => await EntityFrameworkQueryableExtensions.LongCountAsync((IQueryable<T>) queryable));
            return shardingCounts.Sum();
        }


        public async Task<List<T>> ToListAsync()
        {
            return await GetShardingListQueryAsync();
        }


        public async Task<T> FirstOrDefaultAsync()
        {
            var result = await GetShardingQueryAsync(async queryable => await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync((IQueryable<T>) queryable));

            var q = result.Where(o => o != null).AsQueryable();
            var extraEntry = _source.GetExtraEntry();
            if (extraEntry.Orders.Any())
                q = q.OrderWithExpression(extraEntry.Orders);

            return q.FirstOrDefault();
        }

        public async Task<bool> AnyAsync()
        {
            return (await GetShardingQueryAsync(x => EntityFrameworkQueryableExtensions.AnyAsync((IQueryable<T>) x)))
                .Any(o => o);
        }

        public async Task<T> MaxAsync()
        {
            var results = await GetShardingQueryAsync(async queryable => await EntityFrameworkQueryableExtensions.MaxAsync((IQueryable<T>) queryable));
            return results.Max();
        }

        public async Task<T> MinAsync()
        {
            var results = await GetShardingQueryAsync(async queryable => await EntityFrameworkQueryableExtensions.MinAsync((IQueryable<T>) queryable));
            return results.Min();
        }

        public async Task<int> SumAsync()
        {
            if (typeof(T) != typeof(int))
                throw new InvalidOperationException($"{typeof(T)} cast to int failed");
            var results = await GetShardingQueryAsync(async queryable => await EntityFrameworkQueryableExtensions.SumAsync((IQueryable<int>) queryable));
            return results.Sum();
        }

        public async Task<long> LongSumAsync()
        {
            if (typeof(T) != typeof(long))
                throw new InvalidOperationException($"{typeof(T)} cast to long failed");
            var results = await GetShardingQueryAsync(async queryable => await EntityFrameworkQueryableExtensions.SumAsync((IQueryable<long>) queryable));
            return results.Sum();
        }

        public async Task<decimal> DecimalSumAsync()
        {
            if (typeof(T) != typeof(decimal))
                throw new InvalidOperationException($"{typeof(T)} cast to decimal failed");
            var results = await GetShardingQueryAsync(async queryable => await EntityFrameworkQueryableExtensions.SumAsync((IQueryable<decimal>) queryable));
            return results.Sum();
        }

        public async Task<double> DoubleSumAsync()
        {
            if (typeof(T) != typeof(double))
                throw new InvalidOperationException($"{typeof(T)} cast to double failed");
            var results = await GetShardingQueryAsync(async queryable => await EntityFrameworkQueryableExtensions.SumAsync((IQueryable<double>) queryable));
            return results.Sum();
        }

        public async Task<float> FloatSumAsync()
        {
            if (typeof(T) != typeof(float))
                throw new InvalidOperationException($"{typeof(T)} cast to double failed");
            var results = await GetShardingQueryAsync(async queryable => await EntityFrameworkQueryableExtensions.SumAsync((IQueryable<float>) queryable));
            return results.Sum();
        }


        private void BeginShardingQuery(ShardingScope scope)
        {
            var context = ShardingContext.Create();
            foreach (var route in _endRoutes)
            {
                context.TryAddShardingTable(route.Key, route.Value);
            }

            scope.ShardingAccessor.ShardingContext = context;
        }

        private void GetQueryableRoutes()
        {
            //先添加手动路由到当前上下文,之后将不再手动路由里面的自动路由添加到当前上下文
            foreach (var kv in _routes)
            {
                var virtualTable = _virtualTableManager.GetVirtualTable(kv.Key);
                if (!_endRoutes.ContainsKey(virtualTable))
                {
                    var physicTables = virtualTable.RouteTo(new RouteConfig(null, null, null, kv.Value));
                    _endRoutes.Add(virtualTable, physicTables.Select(o => o.Tail).ToList());
                }
            }

            if (_autoParseRoute)
            {
                var shardingEntities = _source.ParseQueryableRoute();
                var autoRoutes = shardingEntities.Where(o => !_routes.ContainsKey(o)).ToList();
                foreach (var shardingEntity in autoRoutes)
                {
                    var virtualTable = _virtualTableManager.GetVirtualTable(shardingEntity);

                    if (!_endRoutes.ContainsKey(virtualTable))
                    {
                        //路由获取本次操作物理表
                        var physicTables = virtualTable.RouteTo(new RouteConfig(_source));
                        _endRoutes.Add(virtualTable, physicTables.Select(o => o.Tail).ToList());
                    }
                }
            }
        }

        private async Task<List<TResult>> GetShardingQueryAsync<TResult>(Func<IQueryable, Task<TResult>> efQuery)
        {
            GetQueryableRoutes();
            //本次查询仅一张表是对应多个数据源的情况
            if (_endRoutes.Values.Count(o => o.Count > 1) == 1)
            {
                return await GetShardingMultiQueryAsync(efQuery);
            }
            else
            {
#if DEBUG
                if (_endRoutes.Values.Count(o => o.Count > 1) > 1)
                {
                    Console.WriteLine("存在性能可能有问题");
                }
#endif
                   
                var result= await GetShardingSingleQueryAsync(efQuery);
                return result;
            }
        }

        private async Task<List<TResult>> GetShardingSingleQueryAsync<TResult>(Func<IQueryable, Task<TResult>> efQuery)
        {
            using (var scope = _shardingScopeFactory.CreateScope())
            {
                BeginShardingQuery(scope);
                return new List<TResult>() {await efQuery(_source)};
            }
        }

        private async Task<List<TResult>> GetShardingMultiQueryAsync<TResult>(Func<IQueryable, Task<TResult>> efQuery)
        {
            var extraEntry = _source.GetExtraEntry();
            //去除分页,获取前Take+Skip数量
            int? take = extraEntry.Take;
            int skip = extraEntry.Skip.GetValueOrDefault();

            var noPageSource = _source.RemoveTake().RemoveSkip();
            if (take.HasValue)
                noPageSource = noPageSource.Take(take.Value + skip);
            //从各个分表获取数据
            var multiRouteEntry = _endRoutes.FirstOrDefault(o => o.Value.Count() > 1);
            var tasks = multiRouteEntry.Value.Select(tail =>
            {
                return Task.Run(async () =>
                {
                    using (var shardingDbContext = _shardingParallelDbContextFactory.Create(string.Empty))
                    {
                        var newQ = (IQueryable<T>) noPageSource.ReplaceDbContextQueryable(shardingDbContext);
                        var shardingQueryable = new ShardingQueryable<T>(newQ);
                        shardingQueryable.DisableAutoRouteParse();
                        shardingQueryable.AddManualRoute(multiRouteEntry.Key, tail);
                        foreach (var singleRouteEntry in _endRoutes.Where(o => o.Key != multiRouteEntry.Key))
                        {
                            shardingQueryable.AddManualRoute(singleRouteEntry.Key, singleRouteEntry.Value[0]);
                        }

                        return await shardingQueryable.GetShardingQueryAsync(efQuery);
                    }
                });
            }).ToArray();
            var all = (await Task.WhenAll(tasks)).SelectMany(o => o).ToList();
            //合并数据
            var resList = all;
            if (extraEntry.Orders.Any())
                resList = resList.AsQueryable().OrderWithExpression(extraEntry.Orders).ToList();
            if (skip > 0)
                resList = resList.Skip(skip).ToList();
            if (take.HasValue)
                resList = resList.Take(take.Value).ToList();
            return resList;
        }


        #region 处理list

        private async Task<List<T>> GetShardingListQueryAsync()
        {
            GetQueryableRoutes();
            //本次查询仅一张表是对应多个数据源的情况
            if (_endRoutes.Values.Count(o => o.Count > 1) == 1)
            {
                return await GetShardingMultiListQueryAsync();
            }
            else
            {
#if DEBUG
                if (_endRoutes.Values.Count(o => o.Count > 1) > 1)
                {
                    Console.WriteLine("存在性能可能有问题");
                }
#endif
                return await GetShardingSingleListQueryAsync();
            }
        }

        private async Task<IAsyncEnumerator<T>> GetAsyncEnumerator()
        {   
            using (var scope = _shardingScopeFactory.CreateScope())
            {
                BeginShardingQuery(scope);
#if !EFCORE2
            
                var enumator = _source.AsAsyncEnumerable().GetAsyncEnumerator();
                await enumator.MoveNextAsync();    
#endif
#if EFCORE2
            
                var enumator = _source.AsAsyncEnumerable().GetEnumerator();
                await enumator.MoveNext();    
#endif
                return enumator;
            }
        }

        private async Task<List<T>> GetShardingSingleListQueryAsync()
        {
            using (var scope = _shardingScopeFactory.CreateScope())
            {
                BeginShardingQuery(scope);
                return await _source.ToListAsync();
            }
        }

        private async Task<List<T>> GetShardingMultiListQueryAsync()
        {
            var extraEntry = _source.GetExtraEntry();
            //去除分页,获取前Take+Skip数量
            int? take = extraEntry.Take;
            int skip = extraEntry.Skip.GetValueOrDefault();

            var noPageSource = _source.RemoveTake().RemoveSkip();
            if (take.HasValue)
                noPageSource = noPageSource.Take(take.Value + skip);
            //从各个分表获取数据
            var multiRouteEntry = _endRoutes.FirstOrDefault(o => o.Value.Count() > 1);
            List<DbContext> parallelDbContexts = new List<DbContext>(multiRouteEntry.Value.Count);
           var enumatorTasks= multiRouteEntry.Value.Select(tail =>
           {
               return Task.Run(async () =>
               {
                   var shardingDbContext = _shardingParallelDbContextFactory.Create(string.Empty);
                   parallelDbContexts.Add(shardingDbContext);
                   var newQ = (IQueryable<T>) noPageSource.ReplaceDbContextQueryable(shardingDbContext);
                   var shardingQueryable = new ShardingQueryable<T>(newQ);
                   shardingQueryable.DisableAutoRouteParse();
                   shardingQueryable.AddManualRoute(multiRouteEntry.Key, tail);
                   foreach (var singleRouteEntry in _endRoutes.Where(o => o.Key != multiRouteEntry.Key))
                   {
                       shardingQueryable.AddManualRoute(singleRouteEntry.Key, singleRouteEntry.Value[0]);
                   }
#if !EFCORE2
                   return await shardingQueryable.GetAsyncEnumerator();
#endif
#if EFCORE2

                return await shardingQueryable.GetAsyncEnumerator();
#endif
               });
           }).ToArray();
           var enumators = (await Task.WhenAll(enumatorTasks)).ToList();
            var engine=new StreamMergeListEngine<T>(new StreamMergeContext(extraEntry.Skip,extraEntry.Take,extraEntry.Orders), enumators);

            var result= await engine.Execute();
            parallelDbContexts.ForEach(o=>o.Dispose());
            return result;
        }

        #endregion

        // private async Task<TResult> GetShardingMultiQueryAsync<TResult>(Func<IQueryable, Task<TResult>> efQuery)
        // {
        //     //去除分页,获取前Take+Skip数量
        //     int? take = _source.GetTakeCount();
        //     int skip = _source.GetSkipCount();
        //     var (sortColumn, sortType) = _source.GetOrderBy();
        //
        //     var noPageSource = _source.RemoveTake().RemoveSkip();
        //     if (take.HasValue)
        //         noPageSource = noPageSource.Take(take.Value + skip);
        //
        //     //从各个分表获取数据
        //     var multiRouteEntry = _endRoutes.FirstOrDefault(o => o.Value.Count() > 1);
        //     var tasks = multiRouteEntry.Value.Select(tail =>
        //     {
        //         return Task.Run(async () =>
        //         {
        //             using (var shardingDbContext = _shardingParallelDbContextFactory.Create(string.Empty))
        //             {
        //                 var newQ = (IQueryable<T>) noPageSource.ReplaceDbContextQueryable(shardingDbContext);
        //                 var shardingQueryable = new ShardingQueryable<T>(newQ);
        //                 shardingQueryable.AddManualRoute(multiRouteEntry.Key, tail);
        //                 foreach (var singleRouteEntry in _endRoutes.Where(o => o.Key != multiRouteEntry.Key))
        //                 {
        //                     shardingQueryable.AddManualRoute(singleRouteEntry.Key, singleRouteEntry.Value[0]);
        //                 }
        //
        //                 return await shardingQueryable.GetShardingQueryAsync(efQuery);
        //             }
        //         });
        //     }).ToArray();
        //     var all=(await Task.WhenAll(tasks)).ToList().SelectMany(o=>o).ToList();
        //
        //     //合并数据
        //     var resList = all;
        //     if (!sortColumn.IsNullOrEmpty() && !sortType.IsNullOrEmpty())
        //         resList = resList.AsQueryable().OrderBy($"{sortColumn} {sortType}").ToList();
        //     if (skip > 0)
        //         resList = resList.Skip(skip).ToList();
        //     if (take.HasValue)
        //         resList = resList.Take(take.Value).ToList();
        //
        //     return resList;
        // }
    }
}