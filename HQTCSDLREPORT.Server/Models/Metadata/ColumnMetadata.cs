namespace HQTCSDL.Models.Metadata
{
   public class ColumnMetadata
    {
        public int ColumnId { get; set; }

        public string ColumnName { get; set; }

        public string DataType { get; set; }

        public int MaxLength { get; set; }

        public bool IsNullable { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsIdentity { get; set; }

        public string TableName { get; set; }
    }
}
