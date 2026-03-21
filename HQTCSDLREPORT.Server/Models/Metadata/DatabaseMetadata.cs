namespace HQTCSDL.Models.Metadata
{
    public class DatabaseMetadata
    {
        public string DatabaseName { get; set; }

        public List<TableMetadata> Tables { get; set; } = new List<TableMetadata>();

    }
}
