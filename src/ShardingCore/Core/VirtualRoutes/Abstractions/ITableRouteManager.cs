using System;
using System.Collections.Generic;
using ShardingCore.Core.VirtualRoutes.TableRoutes;

namespace ShardingCore.Core.VirtualRoutes.Abstractions
{
    
    public interface ITableRouteManager
    {
        bool HasRoute(Type entityType);
        IVirtualTableRoute GetRoute(Type entityType);
        List<IVirtualTableRoute> GetRoutes();
    }
}
