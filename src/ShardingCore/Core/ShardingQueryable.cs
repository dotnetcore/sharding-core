using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.Internal.RoutingRuleEngines;
using ShardingCore.Core.Internal.StreamMerge;
using ShardingCore.Core.Internal.StreamMerge.GenericMerges;
using ShardingCore.Core.VirtualTables;
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
        private readonly IStreamMergeContextFactory _streamMergeContextFactory;
        private Dictionary<Type, Expression> _routes = new Dictionary<Type, Expression>();
        private readonly Dictionary<IVirtualTable, List<string>> _endRoutes = new Dictionary<IVirtualTable, List<string>>();
        private readonly IRoutingRuleEngineFactory _routingRuleEngineFactory;


        private ShardingQueryable(IQueryable<T> source)
        {
            _source = source;
            _streamMergeContextFactory = ShardingContainer.Services.GetService<IStreamMergeContextFactory>();
            _routingRuleEngineFactory=ShardingContainer.Services.GetService<IRoutingRuleEngineFactory>();
        }

        public static ShardingQueryable<TSource> Create<TSource>(IQueryable<TSource> source)
        {
            return new ShardingQueryable<TSource>(source);
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


        private StreamMergeContext<T> GetContext()
        {
            return _streamMergeContextFactory.Create(_source);
        }
        private async Task<List<TResult>> GetGenericMergeEngine<TResult>(Func<IQueryable, Task<TResult>> efQuery)
        {
            return await GenericInMemoryMergeEngine<T>.Create(GetContext()).ExecuteAsync(efQuery);
        }

        public async Task<int> CountAsync()
        {
            var shardingCounts = await GetGenericMergeEngine(async queryable => await EntityFrameworkQueryableExtensions.CountAsync((IQueryable<T>) queryable));
            return shardingCounts.Sum();
        }

        public async Task<long> LongCountAsync()
        {
            var shardingCounts = await GetGenericMergeEngine(async queryable => await EntityFrameworkQueryableExtensions.LongCountAsync((IQueryable<T>) queryable));
            return shardingCounts.Sum();
        }


        public async Task<List<T>> ToListAsync(int capacity=20)
        {
            var context = GetContext();
            using (var engine =  GenericStreamMergeProxyEngine<T>.Create(context))
            {
                return await engine.ToListAsync(capacity);
            }
        }


        public async Task<T> FirstOrDefaultAsync()
        {
            var context = GetContext();
            var result= await GenericInMemoryMergeEngine<T>.Create(context).ExecuteAsync(async queryable => await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync((IQueryable<T>) queryable));
            var q = result.Where(o => o != null).AsQueryable();
            if (context.Orders.Any())
                return  q.OrderWithExpression(context.Orders).FirstOrDefault();

            return q.FirstOrDefault();
            
        }

        public async Task<bool> AnyAsync()
        {
            return (await GetGenericMergeEngine(x => EntityFrameworkQueryableExtensions.AnyAsync((IQueryable<T>) x)))
                .Any(o => o);
        }


        public async Task<T> MaxAsync()
        {
            var results = await GetGenericMergeEngine(async queryable => await EntityFrameworkQueryableExtensions.MaxAsync((IQueryable<T>) queryable));
            return results.Max();
        }

        public async Task<T> MinAsync()
        {
            var results = await GetGenericMergeEngine(async queryable => await EntityFrameworkQueryableExtensions.MinAsync((IQueryable<T>) queryable));
            return results.Min();
        }

        public async Task<int> SumAsync()
        {
            if (typeof(T) != typeof(int))
                throw new InvalidOperationException($"{typeof(T)} cast to int failed");
            var results = await GetGenericMergeEngine(async queryable => await EntityFrameworkQueryableExtensions.SumAsync((IQueryable<int>) queryable));
            return results.Sum();
        }

        public async Task<long> LongSumAsync()
        {
            if (typeof(T) != typeof(long))
                throw new InvalidOperationException($"{typeof(T)} cast to long failed");
            var results = await GetGenericMergeEngine(async queryable => await EntityFrameworkQueryableExtensions.SumAsync((IQueryable<long>) queryable));
            return results.Sum();
        }

        public async Task<decimal> DecimalSumAsync()
        {
            if (typeof(T) != typeof(decimal))
                throw new InvalidOperationException($"{typeof(T)} cast to decimal failed");
            var results = await GetGenericMergeEngine(async queryable => await EntityFrameworkQueryableExtensions.SumAsync((IQueryable<decimal>) queryable));
            return results.Sum();
        }

        public async Task<double> DoubleSumAsync()
        {
            if (typeof(T) != typeof(double))
                throw new InvalidOperationException($"{typeof(T)} cast to double failed");
            var results = await GetGenericMergeEngine(async queryable => await EntityFrameworkQueryableExtensions.SumAsync((IQueryable<double>) queryable));
            return results.Sum();
        }

        public async Task<float> FloatSumAsync()
        {
            if (typeof(T) != typeof(float))
                throw new InvalidOperationException($"{typeof(T)} cast to double failed");
            var results = await GetGenericMergeEngine(async queryable => await EntityFrameworkQueryableExtensions.SumAsync((IQueryable<float>) queryable));
            return results.Sum();
        }

        public async Task<decimal> DecimalAverageAsync()
        {
            if (typeof(T) != typeof(decimal))
                throw new InvalidOperationException($"{typeof(T)} cast to decimal failed");
            var results = await GetGenericMergeEngine(async queryable => await EntityFrameworkQueryableExtensions.AverageAsync((IQueryable<decimal>) queryable));
            return results.Sum()/results.Count();
        }

        public async Task<double> DoubleAverageAsync()
        {
            if (typeof(T) != typeof(double))
                throw new InvalidOperationException($"{typeof(T)} cast to double failed");
            var results = await GetGenericMergeEngine(async queryable => await EntityFrameworkQueryableExtensions.AverageAsync((IQueryable<double>) queryable));
            return results.Sum()/results.Count();
        }

        public async  Task<float> FloatAverageAsync()
        {
            if (typeof(T) != typeof(float))
                throw new InvalidOperationException($"{typeof(T)} cast to float failed");
            var results = await GetGenericMergeEngine(async queryable => await EntityFrameworkQueryableExtensions.AverageAsync((IQueryable<float>) queryable));
            return results.Sum()/results.Count();
        }
    }
}