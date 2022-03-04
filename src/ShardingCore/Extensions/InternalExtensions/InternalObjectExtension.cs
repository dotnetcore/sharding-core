using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Extensions.InternalExtensions
{
    internal static class InternalObjectExtension
    {
        public static T As<T>(this object obj) where T : class
        {
            return (T)obj;
        }
    }
}
