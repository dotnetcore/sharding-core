using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class ShardingCoreNotFoundException:ShardingCoreException
    {
        public ShardingCoreNotFoundException(string message) : base(message)
        {
        }

        public ShardingCoreNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
