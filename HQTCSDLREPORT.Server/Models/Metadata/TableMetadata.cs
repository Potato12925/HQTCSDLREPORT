namespace HQTCSDL.Models.Metadata
{
    public class TableMetadata
    {
        public int ObjectId { get; set; }

        public string TableName { get; set; }

        public string SchemaName { get; set; }

        public List<ColumnMetadata> Columns { get; set; } = new List<ColumnMetadata>();

        public List<ForeignKeyMetadata> ForeignKeys { get; set; } = new List<ForeignKeyMetadata>();
    }
}
