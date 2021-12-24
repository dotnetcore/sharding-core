using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class ShardingCoreDataSourceQueryRouteNotMatchException : ShardingCoreQueryRouteNotMatchException
    {
        public ShardingCoreDataSourceQueryRouteNotMatchException(string message) : base(message)
        {
        }

        public ShardingCoreDataSourceQueryRouteNotMatchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
