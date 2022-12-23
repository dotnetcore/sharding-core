using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class ShardingCoreNotImplementedException:ShardingCoreException
    {
        public ShardingCoreNotImplementedException()
        {
            
        }
        public ShardingCoreNotImplementedException(string message) : base(message)
        {
        }

        public ShardingCoreNotImplementedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
