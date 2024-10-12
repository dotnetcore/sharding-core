#if EFCORE8
using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace ShardingCore.Utils
{
    
    public class Sharding8Util
    {
    
        public static IRelationalDatabaseFacadeDependencies GetFacadeDependencies(
            DatabaseFacade databaseFacade)
        {
            if (!(((IDatabaseFacadeDependenciesAccessor) databaseFacade).Dependencies is IRelationalDatabaseFacadeDependencies dependencies))
                throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
            return dependencies;
        }

        public static bool FromSqlQueryRootTypeInModel(Type entityType,DatabaseFacade databaseFacade)
        {
            var facadeDependencies = GetFacadeDependencies(databaseFacade);
            return facadeDependencies.TypeMappingSource.FindMapping(entityType) != null;
        }
    }
}


#endif