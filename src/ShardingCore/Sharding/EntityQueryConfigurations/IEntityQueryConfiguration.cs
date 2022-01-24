using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.EntityQueryConfigurations
{
    public interface IEntityQueryConfiguration<TEntity> where TEntity:class
    {
        void Configure(EntityQueryBuilder<TEntity> builder);
    }
}
