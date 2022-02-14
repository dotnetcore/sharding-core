using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Samples.AutoByDate.SqlServer.Domain.Entities;

namespace Samples.AutoByDate.SqlServer
{
    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<挂起>")]
    public class RemoveForeignKeyMigrationsModelDiffer : MigrationsModelDiffer
    {
        private readonly ISet<string> _ignoreForeignKeys=new HashSet<string>()
        {
            nameof(SysUserLog1ByDay)
        };
        public RemoveForeignKeyMigrationsModelDiffer(IRelationalTypeMappingSource typeMappingSource, IMigrationsAnnotationProvider migrationsAnnotations, IChangeDetector changeDetector, IUpdateAdapterFactory updateAdapterFactory, CommandBatchPreparerDependencies commandBatchPreparerDependencies) : base(typeMappingSource, migrationsAnnotations, changeDetector, updateAdapterFactory, commandBatchPreparerDependencies)
        {
        }

        public override IReadOnlyList<MigrationOperation> GetDifferences(IRelationalModel? source, IRelationalModel? target)
        {
            var sourceOperations = base.GetDifferences(source, target).ToList();
            sourceOperations.RemoveAll(x => (x is AddForeignKeyOperation addForeignKey && _ignoreForeignKeys.Contains(addForeignKey.Table)) || (x is DropForeignKeyOperation dropForeignKey && _ignoreForeignKeys.Contains(dropForeignKey.Table)));
            foreach (var operation in sourceOperations.OfType<CreateTableOperation>())
            {
                operation.ForeignKeys?.Clear();
            }
            return sourceOperations;
        }
    }
}
