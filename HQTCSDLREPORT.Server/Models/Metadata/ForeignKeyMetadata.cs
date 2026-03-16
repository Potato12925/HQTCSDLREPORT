namespace HQTCSDL.Models.Metadata
{
    public class ForeignKeyMetadata
    {
        public string ForeignKeyName { get; set; }

        public string ParentTable { get; set; }

        public string ParentColumn { get; set; }

        public string ReferencedTable { get; set; }

        public string ReferencedColumn { get; set; }
    }
}
