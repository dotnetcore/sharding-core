using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Extensions;

namespace ShardingCore.Sharding.EntityQueryConfigurations
{
    public class EntityQueryMetadata
    {
        public IDictionary<string, EntitySeqQueryConfig> EntityOrderSeqQueryConfigs { get; }

        public EntityQueryMetadata()
        {
            EntityOrderSeqQueryConfigs = new Dictionary<string, EntitySeqQueryConfig>();//倒叙comparer
        }

        public bool TryGetSeqQueryConfig(string orderPropertyName, out EntitySeqQueryConfig seqQueryConfig)
        {
            if (!string.IsNullOrWhiteSpace(orderPropertyName))
            {
                if (EntityOrderSeqQueryConfigs.TryGetValue(orderPropertyName, out seqQueryConfig))
                {
                    return true;
                }
            }

            seqQueryConfig = null;
            return false;
        }
    }
}
