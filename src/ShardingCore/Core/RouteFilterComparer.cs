using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;

namespace ShardingCore.Core
{
    internal class RouteFilterComparer : IEqualityComparer<Expression<Func<string, bool>>>
    {
        public int Compare(Expression<Func<string, bool>>? x, Expression<Func<string, bool>>? y)
        {
            if (LambdaCompare.Eq(x, y))
                return 0;
            return - 1;
        }

        public bool Equals(Expression<Func<string, bool>>? x, Expression<Func<string, bool>>? y)
        {
            return LambdaCompare.Eq(x, y);
        }

        public int GetHashCode(Expression<Func<string, bool>> obj)
        {
            return 0;
        }
    }
}
