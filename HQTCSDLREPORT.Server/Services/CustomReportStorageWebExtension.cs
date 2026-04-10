using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.ClientControls;
using DevExpress.XtraReports.Web.Extensions;

namespace HQTCSDLREPORT.Server.Services
{
    public class CustomReportStorageWebExtension : ReportStorageWebExtension
    {
        private readonly SqlReportStore _sqlReportStore;

        public CustomReportStorageWebExtension(SqlReportStore sqlReportStore)
        {
            _sqlReportStore = sqlReportStore;
        }

        public override bool CanSetData(string url)
        {
            return false;
        }

        public override bool IsValidUrl(string url)
        {
            return !string.IsNullOrWhiteSpace(url) && url.StartsWith("sql-report-", StringComparison.Ordinal);
        }

        public override byte[] GetData(string url)
        {
            if (!_sqlReportStore.TryGet(url, out var reportItem) || reportItem is null)
            {
                throw new FaultException($"Could not find report '{url}'.");
            }

            using var report = SqlReportDocumentBuilder.Build(reportItem, url);
            using var stream = new MemoryStream();
            report.SaveLayoutToXml(stream);
            return stream.ToArray();
        }

        public override Dictionary<string, string> GetUrls()
        {
            return _sqlReportStore.GetUrls().ToDictionary(x => x.Key, x => x.Value);
        }

        public override void SetData(XtraReport report, string url)
        {
            throw new NotSupportedException("Report storage is read-only for SQL reports.");
        }

        public override string SetNewData(XtraReport report, string defaultUrl)
        {
            throw new NotSupportedException("Creating reports via storage is not supported.");
        }
    }
}
