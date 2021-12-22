using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ShardingCore.EFCores
{
    public interface IShardingModelSource
    {
        IModelCacheKeyFactory GetModelCacheKeyFactory();
        object GetSyncObject();
        void Remove(object key);

    }
}
