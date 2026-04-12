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
            IEnumerable<ReportParameterRequest>? parameters,
            IEnumerable<ReportGroupOrderRequest>? groupOrder)
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
                Parameters = (parameters ?? Enumerable.Empty<ReportParameterRequest>())
                    .Select(x => new ReportParameterRequest
                    {
                        TableId = x.TableId,
                        ColumnId = x.ColumnId,
                        ColumnName = x.ColumnName,
                        Value = x.Value
                    })
                    .ToList(),
                GroupOrder = (groupOrder ?? Enumerable.Empty<ReportGroupOrderRequest>())
                    .Select(x => new ReportGroupOrderRequest
                    {
                        Order = x.Order,
                        TableId = x.TableId,
                        ColumnId = x.ColumnId,
                        ColumnName = x.ColumnName
                    })
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

            public List<ReportParameterRequest> Parameters { get; set; } = new();

            public List<ReportGroupOrderRequest> GroupOrder { get; set; } = new();

            public DateTime CreatedAtUtc { get; set; }
        }
    }
}
