namespace HQTCSDL.Models.Metadata
{
    public class ForeignKeyMetadata
    {
        public string ForeignKeyName { get; set; }

        public int ParentObjectId { get; set; }
        public string ParentTable { get; set; }
        public int ParentColumnId { get; set; }
        public string ParentColumn { get; set; }

        public int ReferencedObjectId { get; set; }
        public string ReferencedTable { get; set; }
        public int ReferencedColumnId { get; set; }
        public string ReferencedColumn { get; set; }
    }
}
