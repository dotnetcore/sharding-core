using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class ShardingCoreInvalidOperationException: ShardingCoreException
    {

        public ShardingCoreInvalidOperationException(string? message) : base(message)
        {
        }

        public ShardingCoreInvalidOperationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
