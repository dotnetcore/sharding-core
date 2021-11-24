using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.DIExtensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Extensions
{
    public static class ShardingCoreConfigBuilderExtension
    {
        [ExcludeFromCodeCoverage]
        public static void AddEntityTryCreateTable<TEntity>(this ShardingCoreBeginOptions source) where TEntity:class
        {
             source.AddEntitiesTryCreateTable(typeof(TEntity));
        }
    }
}
