using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualTables;

namespace ShardingCore.Extensions
{
    public static class VirtualTableManagerExtension
    {
        public static IVirtualTable<TEntity> GetVirtualTable<TEntity>(this IVirtualTableManager virtualTableManager) where TEntity:class
        {
            return (IVirtualTable<TEntity>)virtualTableManager.GetVirtualTable(typeof(TEntity));
        }
    }
}
