//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;
//using ShardingCore.Core.EntityMetadatas;
//using ShardingCore.Core.PhysicTables;
//using ShardingCore.Extensions;
//using ShardingCore.Sharding.Visitors;

//namespace ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions
//{
//    /// <summary>
//    /// 路由解析缓存
//    /// </summary>
//    /// <typeparam name="TEntity"></typeparam>
//    /// <typeparam name="TKey"></typeparam>
//    public abstract class AbstractShardingRouteParseCompileCacheVirtualTableRoute<TEntity, TKey> : AbstractShardingFilterVirtualTableRoute<TEntity, TKey> where TEntity : class
//    {
//        private  readonly ConcurrentDictionary<Expression<Func<string, bool>>, Func<string, bool>> _routeCompileCaches = new(new ExtensionExpressionComparer.RouteParseExpressionEqualityComparer());

//        protected AbstractShardingRouteParseCompileCacheVirtualTableRoute()
//        {
//            Expression<Func<string, bool>> defaultWhere1 = x => true;
//            _routeCompileCaches.TryAdd(defaultWhere1, defaultWhere1.Compile());
//            Expression<Func<string, bool>> defaultWhere2 = x => true;
//            var expression = defaultWhere2.And(defaultWhere1);
//            _routeCompileCaches.TryAdd(expression, expression.Compile());
//        }
//        /// <summary>
//        /// 是否启用路由解析编译缓存
//        /// </summary>
//        public virtual bool? EnableRouteParseCompileCache => null;
//        public virtual bool EnableCompileCache()
//        {
//            if (EnableRouteParseCompileCache.HasValue)
//                return EnableRouteParseCompileCache.Value;
//            return RouteConfigOptions.EnableTableRouteCompileCache.GetValueOrDefault();
//        }
//        /// <summary>
//        /// 对表达式进行缓存编译默认永久缓存单个参数表达式，且不包含orElse只包含单个AndAlso或者没有AndAlso的,
//        /// 比如:<![CDATA[o.id==x]]>或者<![CDATA[o.id>x]]>,不会缓存<![CDATA[o=>id>x && o.id<y ]]>等一共大于、等于、小于、大于等于、小于等于(不等于编译成<![CDATA[t=>true]]>)缓存会存在的数量个数上限为
//        /// 表后缀x*5+2，当前表如果有300个后缀那么就是1502个缓存结果额外两个为<![CDATA[o=>true]]>和<![CDATA[o=>true and true]]>
//        /// </summary>
//        /// <param name="parseWhere"></param>
//        /// <returns></returns>
//        public virtual Func<string, bool> CachingCompile(Expression<Func<string, bool>> parseWhere)
//        {
//            if (EnableCompileCache())
//            {
//                var doCachingCompile = DoCachingCompile(parseWhere);
//                if (doCachingCompile != null)
//                    return doCachingCompile;
//                doCachingCompile = CustomerCachingCompile(parseWhere);
//                if (doCachingCompile != null)
//                    return doCachingCompile;
//            }
//            return parseWhere.Compile();
//        }
//        /// <summary>
//        /// 系统默认永久单表达式缓存
//        /// </summary>
//        /// <param name="parseWhere"></param>
//        /// <returns>返回null会走<see cref="CustomerCachingCompile"/>这个方法如果还是null就会调用<see cref="LambdaExpression.Compile()"/>方法</returns>
//        protected virtual Func<string, bool> DoCachingCompile(Expression<Func<string, bool>> parseWhere)
//        {
//            var shouldCache = ShouldCache(parseWhere);
//            if(shouldCache)
//                return _routeCompileCaches.GetOrAdd(parseWhere, key => parseWhere.Compile());
//            return null;
//        }
//        /// <summary>
//        /// 表达式是否应该被缓存默认没有or并且and只有一个或者没有
//        /// </summary>
//        /// <param name="whereExpression"></param>
//        /// <returns></returns>
//        protected bool ShouldCache(Expression whereExpression)
//        {
//            var routeParseCacheExpressionVisitor = new RouteParseCacheExpressionVisitor();
//            routeParseCacheExpressionVisitor.Visit(whereExpression);
//            if (routeParseCacheExpressionVisitor.HasOrElse())
//                return false;
//            if (routeParseCacheExpressionVisitor.AndAlsoCount() > 1)
//                return false;
//            return true;
//        }

//        protected virtual Func<string, bool> CustomerCachingCompile(Expression<Func<string, bool>> parseWhere)
//        {
//            return null;
//        }
//    }
//}
