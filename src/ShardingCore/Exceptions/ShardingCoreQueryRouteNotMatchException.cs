using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class ShardingCoreQueryRouteNotMatchException : ShardingCoreException
    {
        public ShardingCoreQueryRouteNotMatchException(string message) : base(message)
        {
        }

        public ShardingCoreQueryRouteNotMatchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
