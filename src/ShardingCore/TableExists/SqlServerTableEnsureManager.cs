using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.TableExists.Abstractions;
using System.Collections.Generic;
using System.Data.Common;

namespace ShardingCore.TableExists
{
    public  class SqlServerTableEnsureManager<TShardingDbContext> : AbstractTableEnsureManager<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private const string Tables = "Tables";
        private const string TABLE_NAME = "TABLE_NAME";

        public override ISet<string> DoGetExistTables(DbConnection connection, string dataSourceName)
        {
            ISet<string> result = new HashSet<string>();
            using (var dataTable = connection.GetSchema(Tables))
            {
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    result.Add(dataTable.Rows[i][TABLE_NAME].ToString());
                }
            }
            return result;
        }
    }
}
