namespace HQTCSDLREPORT.Server.Models.Metadata
{
    public class QueryMetadata
    {

        public const String getDatabaseQuery = @"
            SELECT 
                name as databaseName
            FROM sys.databases
            WHERE 
                database_id > 4    
                AND state = 0     
            ORDER BY name;
        ";

        public const String getTablesQuery = @"
            SELECT 
                t.object_id AS ObjectId,
                t.name AS TableName,
                s.name AS SchemaName
            FROM sys.tables t
            JOIN sys.schemas s 
                ON t.schema_id = s.schema_id
            WHERE t.name NOT LIKE 'sys%' 
              AND t.name NOT LIKE 'MS%'
        ";

        public const String getForeighKey = @"
            SELECT 
                fk.name AS ForeignKeyName,

                fkc.parent_object_id AS ParentObjectId,
                parent_t.name AS ParentTable,
                parent_c.column_id AS ParentColumnId,
                parent_c.name AS ParentColumn,

                fkc.referenced_object_id AS ReferencedObjectId,
                ref_t.name AS ReferencedTable,
                ref_c.column_id AS ReferencedColumnId,
                ref_c.name AS ReferencedColumn

            FROM sys.foreign_keys fk

            JOIN sys.foreign_key_columns fkc 
                ON fk.object_id = fkc.constraint_object_id

            JOIN sys.tables parent_t 
                ON fkc.parent_object_id = parent_t.object_id

            JOIN sys.columns parent_c 
                ON fkc.parent_object_id = parent_c.object_id 
                AND fkc.parent_column_id = parent_c.column_id

            JOIN sys.tables ref_t 
                ON fkc.referenced_object_id = ref_t.object_id

            JOIN sys.columns ref_c 
                ON fkc.referenced_object_id = ref_c.object_id 
                AND fkc.referenced_column_id = ref_c.column_id

            WHERE 
                parent_t.type = 'U'
                AND ref_t.type = 'U'

                AND parent_t.name NOT LIKE 'sys%' 
                AND parent_t.name NOT LIKE 'MS%'

                AND ref_t.name NOT LIKE 'sys%' 
                AND ref_t.name NOT LIKE 'MS%'
        ";

        public const String getAllTablesQuery = @"
            SELECT 
                c.object_id AS ObjectId,
                tb.name AS TableName,
                c.column_id AS ColumnId,
                c.name AS ColumnName,
                t.name AS DataType
            FROM sys.columns c
            JOIN sys.tables tb 
                ON tb.object_id = c.object_id
            JOIN sys.types t 
                ON c.user_type_id = t.user_type_id
            WHERE tb.name NOT LIKE 'sys%' 
            AND tb.name NOT LIKE 'MS%' 
            ORDER BY c.object_id, c.column_id
        ";
    }
}
