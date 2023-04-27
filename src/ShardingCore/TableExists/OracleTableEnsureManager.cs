using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.TableExists.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace ShardingCore.TableExists
{
    public class OracleTableEnsureManager : AbstractTableEnsureManager
    {
        private const string Tables = "Tables";
        private const string TABLE_NAME = "TABLE_NAME";

        public OracleTableEnsureManager(IRouteTailFactory routeTailFactory) : base(routeTailFactory)
        {
        }

        public override ISet<string> DoGetExistTables(DbConnection connection, string dataSourceName)
        {
            ISet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (DataTable dataTable = connection.GetSchema(Tables))
            {
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    string schema = dataTable.Rows[i][TABLE_NAME]?.ToString();
                    result.Add(schema);
                }
            }
            return result;
        }
    }
}
