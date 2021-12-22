using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class ShardingCoreNotSupportException : ShardingCoreException
    {
        public ShardingCoreNotSupportException(string message) : base(message)
        {
        }

        public ShardingCoreNotSupportException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
