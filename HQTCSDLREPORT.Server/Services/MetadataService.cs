using HQTCSDL.Models;
using HQTCSDL.Models.Metadata;
using HQTCSDLREPORT.Server.Models.Metadata;
using Microsoft.Data.SqlClient;

namespace HQTCSDL.Services
{
    public class MetadataService
    {

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
                            // tránh duplicate FK (nếu multi-column)
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
    }
}