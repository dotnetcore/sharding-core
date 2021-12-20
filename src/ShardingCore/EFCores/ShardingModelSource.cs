using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding.Abstractions;
#if !EFCORE2
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
#endif

namespace ShardingCore.EFCores
{
    public class ShardingModelSource:ModelSource
    {
        public ShardingModelSource(ModelSourceDependencies dependencies) : base(dependencies)
        {
        }
#if !EFCORE2&&!EFCORE3&&!EFCORE5&&!EFCORE6
        1
#endif

#if EFCORE6

        public override IModel GetModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
        {
            if (context is IShardingTableDbContext shardingTableDbContext)
            {
                if (shardingTableDbContext.RouteTail is IMultiQueryRouteTail)
                {
                   var model = this.CreateModel(context, modelCreationDependencies.ConventionSetBuilder, modelCreationDependencies.ModelDependencies);
                    model = modelCreationDependencies.ModelRuntimeInitializer.Initialize(model, designTime, modelCreationDependencies.ValidationLogger);
                    return model;
                }
            }
            return base.GetModel(context, modelCreationDependencies, designTime);
        }
#endif
#if EFCORE5
        public override IModel GetModel(DbContext context, IConventionSetBuilder conventionSetBuilder, ModelDependencies modelDependencies)
        {
            if (context is IShardingTableDbContext shardingTableDbContext)
            {
                if (shardingTableDbContext.RouteTail is IMultiQueryRouteTail)
                {
                   var model = this.CreateModel(context, conventionSetBuilder, modelDependencies);
                   return model;
                }
            }
            return base.GetModel(context, conventionSetBuilder, modelDependencies);
        }
#endif
#if EFCORE3
        public override IModel GetModel(DbContext context, IConventionSetBuilder conventionSetBuilder)
        {
            if (context is IShardingTableDbContext shardingTableDbContext)
            {
                if (shardingTableDbContext.RouteTail is IMultiQueryRouteTail)
                {
                    var model = this.CreateModel(context, conventionSetBuilder);
                    return model;
                }
            }
            return base.GetModel(context, conventionSetBuilder);
        }
#endif

#if EFCORE2
        public override IModel GetModel(DbContext context, IConventionSetBuilder conventionSetBuilder, IModelValidator validator)
        {
            if (context is IShardingTableDbContext shardingTableDbContext)
            {
                if (shardingTableDbContext.RouteTail is IMultiQueryRouteTail)
                {
                    var model = this.CreateModel(context, conventionSetBuilder, validator);
                    return model;
                }
            }
            return base.GetModel(context, conventionSetBuilder, validator);
        }
#endif
    }
}
