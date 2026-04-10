using DevExpress.DataAccess.Json;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using System.Data;
using System.Text.Json;

namespace HQTCSDLREPORT.Server.Services
{
    public static class SqlReportDocumentBuilder
    {
        public static XtraReport Build(SqlReportStore.SqlReportItem reportItem, string reportUrl)
        {
            var report = new XtraReport
            {
                DisplayName = reportUrl
            };

            var rows = reportItem.DataTable.Rows
                .Cast<DataRow>()
                .Select(row => reportItem.DataTable.Columns
                    .Cast<DataColumn>()
                    .ToDictionary(col => col.ColumnName, col => row[col] == DBNull.Value ? null : row[col]))
                .ToList();

            var json = JsonSerializer.Serialize(rows);
            var jsonDataSource = new JsonDataSource
            {
                Name = "SqlResult",
                JsonSource = new CustomJsonSource(json)
            };
            jsonDataSource.Fill();

            report.ComponentStorage.Add(jsonDataSource);
            report.DataSource = jsonDataSource;

            var printableWidth = report.PageWidth - report.Margins.Left - report.Margins.Right;
            var columnCount = Math.Max(1, reportItem.DataTable.Columns.Count);
            var cellWidth = printableWidth / columnCount;

            var reportHeader = new ReportHeaderBand { HeightF = 58f };
            var title = new XRLabel
            {
                Text = $"SQL Report - {reportItem.Server} / {reportItem.Database}",
                Font = new System.Drawing.Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold),
                BoundsF = new System.Drawing.RectangleF(0, 0, printableWidth, 28),
                TextAlignment = TextAlignment.MiddleLeft
            };

            var subtitle = new XRLabel
            {
                Text = reportItem.Sql,
                Font = new System.Drawing.Font("Consolas", 8.5f, System.Drawing.FontStyle.Regular),
                BoundsF = new System.Drawing.RectangleF(0, 32, printableWidth, 24),
                TextAlignment = TextAlignment.MiddleLeft,
                Multiline = false
            };
            reportHeader.Controls.AddRange(new XRControl[] { title, subtitle });

            var pageHeader = new PageHeaderBand { HeightF = 28f };
            var headerTable = new XRTable
            {
                BoundsF = new System.Drawing.RectangleF(0, 0, printableWidth, 28f),
                Borders = BorderSide.All,
                BorderWidth = 1
            };
            var headerRow = new XRTableRow();

            foreach (DataColumn column in reportItem.DataTable.Columns)
            {
                headerRow.Cells.Add(new XRTableCell
                {
                    Text = column.ColumnName,
                    WidthF = cellWidth,
                    BackColor = System.Drawing.Color.FromArgb(230, 234, 241),
                    Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
                    Padding = new PaddingInfo(4, 4, 2, 2)
                });
            }

            headerTable.Rows.Add(headerRow);
            pageHeader.Controls.Add(headerTable);

            var detailBand = new DetailBand { HeightF = 24f };
            var detailTable = new XRTable
            {
                BoundsF = new System.Drawing.RectangleF(0, 0, printableWidth, 24f),
                Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom,
                BorderWidth = 1
            };
            var detailRow = new XRTableRow();

            foreach (DataColumn column in reportItem.DataTable.Columns)
            {
                var cell = new XRTableCell
                {
                    WidthF = cellWidth,
                    Font = new System.Drawing.Font("Segoe UI", 9f),
                    Padding = new PaddingInfo(4, 4, 2, 2)
                };
                cell.ExpressionBindings.Add(
                    new ExpressionBinding("BeforePrint", "Text", $"[{column.ColumnName}]")
                );

                detailRow.Cells.Add(cell);
            }

            detailTable.Rows.Add(detailRow);
            detailBand.Controls.Add(detailTable);

            var footerBand = new PageFooterBand { HeightF = 24f };
            var pageInfo = new XRPageInfo
            {
                BoundsF = new System.Drawing.RectangleF(0, 0, printableWidth, 24f),
                TextAlignment = TextAlignment.MiddleRight,
                TextFormatString = "Page {0} / {1}"
            };
            footerBand.Controls.Add(pageInfo);

            report.Bands.AddRange(new Band[] { reportHeader, pageHeader, detailBand, footerBand });
            return report;
        }
    }
}
