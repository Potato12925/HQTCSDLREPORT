using HQTCSDL.Models;
using HQTCSDL.Models.Metadata;
using Microsoft.Data.SqlClient;

namespace HQTCSDL.Services
{
    public class MetadataService
    {
        public string BuildConnectionString(DbConnectionModel model)
        {
            return $"Server={model.Server};Database={model.Database};User Id={model.Username};Password={model.Password};TrustServerCertificate=True;";
        }

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

                string query = @"
                    SELECT 
                        t.object_id AS ObjectId,
                        s.name AS SchemaName,
                        t.name AS TableName,
                        c.column_id AS ColumnId,
                        c.name AS ColumnName,
                        ty.name AS DataType,
                        c.max_length AS MaxLength,
                        c.is_nullable AS IsNullable,
                        c.is_identity AS IsIdentity
                    FROM sys.tables t
                    JOIN sys.schemas s 
                        ON t.schema_id = s.schema_id
                    JOIN sys.columns c 
                        ON t.object_id = c.object_id
                    JOIN sys.types ty 
                        ON c.user_type_id = ty.user_type_id
                    WHERE 
                        t.is_ms_shipped = 0
                        AND t.temporal_type = 0
                        AND t.is_external = 0
                        AND t.is_filetable = 0
                        AND t.name <> 'sysdiagrams'
                        AND s.name NOT IN ('cdc')
                    ORDER BY 
                        t.name, 
                        c.column_id;
                ";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string tableName = reader["TableName"].ToString();

                    ColumnMetadata column = new ColumnMetadata
                    {
                        ColumnId = (int)reader["ColumnId"],
                        ColumnName = reader["ColumnName"].ToString(),
                        DataType = reader["DataType"].ToString(),
                        MaxLength = (short)reader["MaxLength"],
                        IsNullable = (bool)reader["IsNullable"],
                        IsIdentity = (bool)reader["IsIdentity"],
                        TableName = tableName
                    };

                    database.Columns.Add(column);

                    var table = database.Tables.FirstOrDefault(t => t.TableName == tableName);

                    if (table == null)
                    {
                        table = new TableMetadata
                        {
                            ObjectId = (int)reader["ObjectId"],
                            TableName = tableName,
                            SchemaName = reader["SchemaName"].ToString()
                        };

                        database.Tables.Add(table);
                    }

                    table.Columns.Add(column);
                }

                reader.Close();
            }

            return database;
        }
    }
}