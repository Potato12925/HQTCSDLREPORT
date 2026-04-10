using HQTCSDL.Models.Metadata;
using HQTCSDL.Models.Report;
using HQTCSDLREPORT.Server.Models.Metadata;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace HQTCSDL.Services
{
    public class MetadataService
    {
        private static readonly Regex ForbiddenCommandRegex = new(
            @"\b(insert|update|delete|drop|alter|create|truncate|merge|exec(?:ute)?|grant|revoke|deny)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public bool TestConnection(string connectionString)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return conn.State == System.Data.ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        public DatabaseMetadata LoadMetadata(string connectionString)
        {
            DatabaseMetadata database = new DatabaseMetadata();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                Dictionary<int, TableMetadata> tableDict = new();

                using (SqlCommand cmd = new SqlCommand(QueryMetadata.getAllTablesQuery, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int objectId = reader.GetInt32(0);
                        string tableName = reader.GetString(1);
                        int columnId = reader.GetInt32(2);
                        string columnName = reader.GetString(3);
                        string dataType = reader.GetString(4);

                        if (!tableDict.ContainsKey(objectId))
                        {
                            var table = new TableMetadata
                            {
                                ObjectId = objectId,
                                TableName = tableName
                            };

                            tableDict[objectId] = table;
                            database.Tables.Add(table);
                        }

                        var column = new ColumnMetadata
                        {
                            ObjectId = objectId,
                            ColumnId = columnId,
                            ColumnName = columnName,
                            DataType = dataType
                        };

                        tableDict[objectId].Columns.Add(column);
                    }
                }

                using (SqlCommand fkCmd = new SqlCommand(QueryMetadata.getForeighKey, conn))
                using (SqlDataReader fkReader = fkCmd.ExecuteReader())
                {
                    while (fkReader.Read())
                    {
                        var fk = new ForeignKeyMetadata
                        {
                            ForeignKeyName = fkReader.GetString(0),

                            ParentObjectId = fkReader.GetInt32(1),
                            ParentTable = fkReader.GetString(2),
                            ParentColumnId = fkReader.GetInt32(3),
                            ParentColumn = fkReader.GetString(4),

                            ReferencedObjectId = fkReader.GetInt32(5),
                            ReferencedTable = fkReader.GetString(6),
                            ReferencedColumnId = fkReader.GetInt32(7),
                            ReferencedColumn = fkReader.GetString(8)
                        };

                        if (tableDict.TryGetValue(fk.ParentObjectId, out var table))
                        {
                            if (!table.ForeignKeys.Any(x =>
                                x.ForeignKeyName == fk.ForeignKeyName &&
                                x.ParentColumnId == fk.ParentColumnId))
                            {
                                table.ForeignKeys.Add(fk);
                            }
                        }
                    }
                }
            }

            return database;
        }

        public ReportResult ExecuteSelectQuery(string connectionString, string sql)
        {
            ValidateSelectOnlyQuery(sql);

            var result = new ReportResult();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result.Columns.Add(reader.GetName(i));
                    }

                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object?>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[result.Columns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        }

                        result.Rows.Add(row);
                    }
                }
            }

            return result;
        }

        private static void ValidateSelectOnlyQuery(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("SQL query cannot be empty.");
            }

            var trimmed = sql.Trim();
            var withoutTrailingSemicolon = trimmed.TrimEnd(';', ' ', '\r', '\n', '\t');

            if (withoutTrailingSemicolon.Contains(';'))
            {
                throw new ArgumentException("Only one SELECT statement is allowed.");
            }

            if (!(withoutTrailingSemicolon.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
                || withoutTrailingSemicolon.StartsWith("WITH", StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Only SELECT queries are allowed.");
            }

            if (ForbiddenCommandRegex.IsMatch(withoutTrailingSemicolon))
            {
                throw new ArgumentException("Data modification commands are not allowed.");
            }
        }
        public DataTable ExecuteSelectQueryAsDataTable(string connectionString, string sql)
        {
            ValidateSelectOnlyQuery(sql);

            var dt = new DataTable("MyData");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }

            return dt;
        }
    }
}
