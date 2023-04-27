﻿using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.TableExists.Abstractions;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace ShardingCore.TableExists
{
    public class GuessTableEnsureManager : AbstractTableEnsureManager
    {
        private readonly MySqlTableEnsureManager _tem_mysql;
        private readonly SqliteTableEnsureManager _tem_sqlite;
        private readonly PostgreSqlTableEnsureManager _tem_pgsql;
        private readonly SqlServerTableEnsureManager _tem_mssql;
        private readonly OracleTableEnsureManager _tem_oracle;

        public GuessTableEnsureManager(IRouteTailFactory routeTailFactory) : base(routeTailFactory)
        {
            _tem_mysql = new MySqlTableEnsureManager(routeTailFactory);
            _tem_sqlite = new SqliteTableEnsureManager(routeTailFactory);
            _tem_pgsql = new PostgreSqlTableEnsureManager(routeTailFactory);
            _tem_mssql = new SqlServerTableEnsureManager(routeTailFactory);
            _tem_oracle = new OracleTableEnsureManager(routeTailFactory);
        }

        public override ISet<string> DoGetExistTables(DbConnection connection, string dataSourceName)
        {
            ISet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var dbctypename = connection.GetType().FullName;
            switch (dbctypename)
            {
                case "Npgsql.NpgsqlConnection":
                    result = _tem_pgsql.DoGetExistTables(connection, dataSourceName);
                    break;

                case "System.Data.Sqlite.SqliteConnection":
                case "Microsoft.Data.Sqlite.SqliteConnection":
                    result = _tem_sqlite.DoGetExistTables(connection, dataSourceName);
                    break;

                case "MySqlConnector.MySqlConnection":
                case "MySql.Data.MySqlClient.MySqlConnection":
                    result = _tem_mysql.DoGetExistTables(connection, dataSourceName);
                    break;

                case "Microsoft.Data.SqlClient.SqlConnection":
                case "System.Data.SqlClient.SqlConnection":
                    result = _tem_mssql.DoGetExistTables(connection, dataSourceName);
                    break;

                case "Oracle.ManagedDataAccess.Client.OracleConnection":
                    result = _tem_oracle.DoGetExistTables(connection, dataSourceName);
                    break;

                default:
                    break;
            }
            return result;
        }
    }
}