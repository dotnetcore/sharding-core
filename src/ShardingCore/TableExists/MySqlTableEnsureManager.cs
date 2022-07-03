using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.TableExists.Abstractions;

namespace ShardingCore.TableExists
{
    public class MySqlTableEnsureManager : AbstractTableEnsureManager
    {

        public MySqlTableEnsureManager(IRouteTailFactory routeTailFactory) : base(routeTailFactory)
        {
        }
        private const string Tables = "Tables";
        private const string TABLE_SCHEMA = "TABLE_SCHEMA";
        private const string TABLE_NAME = "TABLE_NAME";

        public override ISet<string> DoGetExistTables(DbConnection connection, string dataSourceName)
        {
            var database = connection.Database;
            ISet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var dataTable = connection.GetSchema(Tables))
            {
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    var schema = dataTable.Rows[i][TABLE_SCHEMA];
                    if (database.Equals($"{schema}", StringComparison.OrdinalIgnoreCase))
                    {
                        var tableName = dataTable.Rows[i][TABLE_NAME];
                        result.Add($"{tableName}");
                    }
                }
            }
            return result;
        }
    }
}
