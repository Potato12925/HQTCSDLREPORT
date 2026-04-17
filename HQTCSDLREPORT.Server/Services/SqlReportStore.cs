using System.Collections.Concurrent;
using System.Data;
using HQTCSDL.Models.Report;

namespace HQTCSDLREPORT.Server.Services
{
    public class SqlReportStore
    {
        private readonly ConcurrentDictionary<string, SqlReportItem> _reports = new();
        private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(30);

        public string Save(
            DataTable dataTable,
            string sql,
            string server,
            string database,
            string title,
            Dictionary<string, string>? parameters,
            IEnumerable<string>? groupOrder
            )
        {
            CleanupExpired();

            var reportId = $"sql-report-{Guid.NewGuid():N}";

            _reports[reportId] = new SqlReportItem
            {
                DataTable = dataTable.Copy(),
                Sql = sql,
                Server = server,
                Database = database,
                Title = title,

                Parameters = parameters ?? new Dictionary<string, string>(),

                GroupOrder = (groupOrder ?? Enumerable.Empty<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .ToList(),

                CreatedAtUtc = DateTime.UtcNow
            };

            return reportId;
        }

        public bool TryGet(string reportId, out SqlReportItem? reportItem)
        {
            reportItem = null;

            if (!_reports.TryGetValue(reportId, out var found))
            {
                return false;
            }

            if (DateTime.UtcNow - found.CreatedAtUtc > MaxAge)
            {
                _reports.TryRemove(reportId, out _);
                return false;
            }

            reportItem = found;
            return true;
        }

        public IReadOnlyDictionary<string, string> GetUrls()
        {
            CleanupExpired();
            return _reports.Keys.ToDictionary(x => x, x => x);
        }

        private void CleanupExpired()
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _reports
                .Where(x => now - x.Value.CreatedAtUtc > MaxAge)
                .Select(x => x.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _reports.TryRemove(key, out _);
            }
        }

        public class SqlReportItem
        {
            public DataTable DataTable { get; set; } = new();

            public string Sql { get; set; } = string.Empty;

            public string Server { get; set; } = string.Empty;

            public string Database { get; set; } = string.Empty;

            public string Title { get; set; } = string.Empty;

            public Dictionary<string, string>? Parameters { get; set; }

            public List<string> GroupOrder { get; set; } = new();

            public DateTime CreatedAtUtc { get; set; }
        }
    }
}
