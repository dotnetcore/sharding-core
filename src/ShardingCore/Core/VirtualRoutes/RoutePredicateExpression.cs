using System;

namespace ShardingCore.Core.VirtualRoutes
{
    
    public class RoutePredicateExpression
    {
        private static readonly Func<string, bool> _defaultRoutePredicate = tail => true;
        private static readonly Func<string, bool> _defaultFalseRoutePredicate = tail => false;
        private readonly Func<string, bool> _routePredicate;

        public RoutePredicateExpression():this(_defaultRoutePredicate)
        {
        }

        public static RoutePredicateExpression Default => new RoutePredicateExpression();
        public static RoutePredicateExpression DefaultFalse => new RoutePredicateExpression(_defaultFalseRoutePredicate);
        public RoutePredicateExpression(Func<string, bool> routePredicate)
        {
            _routePredicate = routePredicate??throw new ArgumentNullException(nameof(routePredicate));
        }
        public RoutePredicateExpression And(RoutePredicateExpression routePredicateExpression)
        {
            var routePredicate = routePredicateExpression.GetRoutePredicate();
            Func<string, bool> func = tail => _routePredicate(tail)&&routePredicate(tail);
            return new RoutePredicateExpression(func);
        }
        public RoutePredicateExpression Or(RoutePredicateExpression routePredicateExpression)
        {
            var routePredicate = routePredicateExpression.GetRoutePredicate();
            Func<string, bool> func = tail => _routePredicate(tail)||routePredicate(tail);
            return new RoutePredicateExpression(func);
        }

        public Func<string, bool> GetRoutePredicate()
        {
            return _routePredicate;
        }
    }
}
