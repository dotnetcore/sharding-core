using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Exceptions
{
    public class ShardingCoreInvalidOperationException: ShardingCoreException
    {

        public ShardingCoreInvalidOperationException(string? message) : base(message)
        {
        }
    }
}
