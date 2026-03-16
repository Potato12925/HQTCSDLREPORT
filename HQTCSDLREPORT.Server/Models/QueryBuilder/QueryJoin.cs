namespace HQTCSDL.Models.QueryBuilder
{
    public class QueryJoin
    {
        public string ParentTable { get; set; }

        public string ParentColumn { get; set; }

        public string ChildTable { get; set; }

        public string ChildColumn { get; set; }

        public string JoinType { get; set; }
    }
}
