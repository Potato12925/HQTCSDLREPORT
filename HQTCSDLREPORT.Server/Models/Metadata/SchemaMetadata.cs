namespace HQTCSDL.Models.Metadata
{
    public class SchemaMetadata
    {
        public int SchemaId { get; set; }

        public string SchemaName { get; set; }

        public List<TableMetadata> Tables { get; set; } = new List<TableMetadata>();
    }
}
