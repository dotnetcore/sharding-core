using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class ShardingCoreTableQueryRouteNotMatchException : ShardingCoreQueryRouteNotMatchException
    {
        public ShardingCoreTableQueryRouteNotMatchException(string message) : base(message)
        {
        }

        public ShardingCoreTableQueryRouteNotMatchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
