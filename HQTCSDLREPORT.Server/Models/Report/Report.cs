using DevExpress.XtraReports.UI;
using System;
using System.Data;
using System.Drawing;

namespace HQTCSDLREPORT.Server
{
    public partial class Report : XtraReport
    {
        public Report()
        {
            InitializeComponent();
        }

        public Report(DataTable dt) : this()
        {
            ApplyDataTable(dt);
        }

        public Report(DataTable dt, string title, string parameter, List<string> groupBy) : this(dt)
        {
            ApplyHeader(title, parameter,groupBy);
        }

        public void ApplyDataTable(DataTable dt)
        {
            CreateDynamicTable(dt);
        }

        private void CreateDynamicTable(DataTable dt)
        {
            var printableWidth = PageWidth - Margins.Left - Margins.Right;
            var columnCount = Math.Max(1, dt.Columns.Count);
            var cellWidth = printableWidth / columnCount;

            var headerTable = new XRTable
            {
                BoundsF = new RectangleF(0, 0, printableWidth, 28f)
            };
            var headerRow = new XRTableRow();

            foreach (DataColumn col in dt.Columns)
            {
                var cell = new XRTableCell
                {
                    Text = col.ColumnName,
                    BackColor = Color.LightGray,
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    WidthF = cellWidth
                };
                headerRow.Cells.Add(cell);
            }

            headerTable.Rows.Add(headerRow);
            TopMargin.Controls.Clear();
            TopMargin.HeightF = 30f;
            TopMargin.Controls.Add(headerTable);

            var detailTable = new XRTable
            {
                BoundsF = new RectangleF(0, 0, printableWidth, 24f)
            };
            var detailRow = new XRTableRow();

            foreach (DataColumn col in dt.Columns)
            {
                var cell = new XRTableCell
                {
                    WidthF = cellWidth
                };
                cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", $"[{col.ColumnName}]"));
                detailRow.Cells.Add(cell);
            }

            detailTable.Rows.Add(detailRow);
            Detail.Controls.Clear();
            Detail.HeightF = 24f;
            Detail.Controls.Add(detailTable);
        }

        private void ApplyHeader(string title, string parameter, List<string> groupBy)
        {
            ReportHeader.Controls.Clear();

            var printableWidth = PageWidth - Margins.Left - Margins.Right;
            float y = 0f;
            const float spacing = 6f;

            if (!string.IsNullOrWhiteSpace(title))
            {
                var titleLabel = new XRLabel
                {
                    Text = title,
                    BoundsF = new RectangleF(0, y, printableWidth, 30f),
                    Font = new Font("Arial", 16, FontStyle.Bold),
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
                };
                ReportHeader.Controls.Add(titleLabel);
                y += titleLabel.HeightF + spacing;
            }

            if (!string.IsNullOrWhiteSpace(parameter))
            {
                var parameterLabel = new XRLabel
                {
                    Text = $"Filter: {parameter}",
                    BoundsF = new RectangleF(0, y, printableWidth, 22f),
                    Font = new Font("Arial", 10),
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft
                };
                ReportHeader.Controls.Add(parameterLabel);
                y += parameterLabel.HeightF + spacing;

                // 👉 APPLY FILTER THẬT
                this.FilterString = parameter;
            }

            if (groupBy != null && groupBy.Any())
            {
                var groupByLabel = new XRLabel
                {
                    Text = $"Group by: {string.Join(", ", groupBy)}",
                    BoundsF = new RectangleF(0, y, printableWidth, 22f),
                    Font = new Font("Arial", 10),
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft
                };
                ReportHeader.Controls.Add(groupByLabel);
                y += groupByLabel.HeightF + spacing;
            }

            ReportHeader.HeightF = y > 0 ? y : 1f;
        }
    }
}
