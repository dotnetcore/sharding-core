using System;

namespace ShardingCore.Core.VirtualRoutes
{
    /// <summary>
    /// 高性能路由条件组合委托
    /// 无需compile支持路由条件直接组合and 和 or
    /// </summary>
    public class RoutePredicateExpression
    {
        private static readonly Func<string, bool> _defaultRoutePredicate = tail => true;
        private static readonly Func<string, bool> _defaultFalseRoutePredicate = tail => false;
        /// <summary>
        /// 默认创建一个true委托
        /// </summary>
        public static RoutePredicateExpression Default => new RoutePredicateExpression();
        /// <summary>
        /// 默认创建一个false委托
        /// </summary>
        public static RoutePredicateExpression DefaultFalse => new RoutePredicateExpression(_defaultFalseRoutePredicate);
        
        
        private readonly Func<string, bool> _routePredicate;

        public RoutePredicateExpression():this(_defaultRoutePredicate)
        {
        }

        public RoutePredicateExpression(Func<string, bool> routePredicate)
        {
            _routePredicate = routePredicate??throw new ArgumentNullException(nameof(routePredicate));
        }
        /// <summary>
        /// and链接当前委托和外部传入的委托
        /// </summary>
        /// <param name="routePredicateExpression"></param>
        /// <returns></returns>
        public RoutePredicateExpression And(RoutePredicateExpression routePredicateExpression)
        {
            var routePredicate = routePredicateExpression.GetRoutePredicate();
            Func<string, bool> func = tail => _routePredicate(tail)&&routePredicate(tail);
            return new RoutePredicateExpression(func);
        }
        /// <summary>
        /// or链接当前委托和外部传入的委托
        /// </summary>
        /// <param name="routePredicateExpression"></param>
        /// <returns></returns>
        public RoutePredicateExpression Or(RoutePredicateExpression routePredicateExpression)
        {
            var routePredicate = routePredicateExpression.GetRoutePredicate();
            Func<string, bool> func = tail => _routePredicate(tail)||routePredicate(tail);
            return new RoutePredicateExpression(func);
        }

        /// <summary>
        /// 返回当前表达式的路由委托条件
        /// </summary>
        /// <returns></returns>
        public Func<string, bool> GetRoutePredicate()
        {
            return _routePredicate;
        }
    }
}
