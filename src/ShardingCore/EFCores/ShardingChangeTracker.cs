using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.EFCores
{
    //public class ShardingChangeTracker: ChangeTracker
    //{
    //    private readonly ICurrentDbContextDiscover _contextDiscover;

    //    public ShardingChangeTracker(DbContext context, IStateManager stateManager, IChangeDetector changeDetector, IModel model, IEntityEntryGraphIterator graphIterator) : base(context, stateManager, changeDetector, model, graphIterator)
    //    {
    //        _contextDiscover = context as ICurrentDbContextDiscover?? throw new ShardingCoreNotSupportException($"{context.GetType()} not impl {nameof(ICurrentDbContextDiscover)}");
    //    }

    //    public override bool HasChanges()
    //    {
    //        return _contextDiscover.GetCurrentDbContexts().Any(o =>
    //            o.Value.GetCurrentContexts().Any(r => r.Value.ChangeTracker.HasChanges()));
    //    }
    //}
}
