namespace HQTCSDL.Models.QueryBuilder
{
    public class QueryBuilderModel
    {
        public string SelectedTable { get; set; }

        public List<string> SelectedColumns { get; set; } = new List<string>();

        public List<QueryFilter> Filters { get; set; } = new List<QueryFilter>();

        public List<QueryJoin> Joins { get; set; } = new List<QueryJoin>();

        public string GeneratedSql { get; set; }
    }
}
