namespace HQTCSDL.Models.Metadata
{
    public class DatabaseMetadata
    {
        public List<SchemaMetadata> Schemas { get; set; } = new List<SchemaMetadata>();

        public List<TableMetadata> Tables { get; set; } = new List<TableMetadata>();

        public List<ColumnMetadata> Columns { get; set; } = new List<ColumnMetadata>();

        public List<ForeignKeyMetadata> ForeignKeys { get; set; } = new List<ForeignKeyMetadata>();
    }
}
