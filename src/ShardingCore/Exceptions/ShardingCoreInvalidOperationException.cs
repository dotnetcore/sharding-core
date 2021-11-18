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
        public ShardingCoreInvalidOperationException()
        {
        }

        protected ShardingCoreInvalidOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ShardingCoreInvalidOperationException(string? message) : base(message)
        {
        }

        public ShardingCoreInvalidOperationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
