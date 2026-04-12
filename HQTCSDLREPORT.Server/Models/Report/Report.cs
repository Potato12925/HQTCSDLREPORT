using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;

namespace HQTCSDLREPORT.Server
{
    public partial class Report : XtraReport
    {
        private static readonly Color PRIMARY = ColorTranslator.FromHtml("#C08552");
        private static readonly Color SECONDARY = ColorTranslator.FromHtml("#8C5A3C");
        private static readonly Color DARK = ColorTranslator.FromHtml("#4B2E2B");
        private static readonly Color LIGHT = ColorTranslator.FromHtml("#FFF8F0");
        public Report()
        {
            InitializeComponent();
        }

        public Report(DataTable dt) : this()
        {
            ApplyDataTable(dt, Array.Empty<string>());
        }

        public Report(DataTable dt, string title, string parameter, List<string> groupBy) : this()
        {
            var normalizedGroups = NormalizeGroupColumns(dt, groupBy);
            ApplyDataTable(dt, normalizedGroups);
            ApplyHeader(title, parameter, normalizedGroups);
        }

        public void ApplyDataTable(DataTable dt, IEnumerable<string> groupColumns)
        {
            CreateDynamicTable(dt, groupColumns);
        }

        private static List<string> NormalizeGroupColumns(DataTable dt, IEnumerable<string>? groupColumns)
        {
            if (groupColumns is null)
            {
                return new List<string>();
            }

            var dataTableColumns = dt.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            return groupColumns
                .Where(x => !string.IsNullOrWhiteSpace(x) && dataTableColumns.Contains(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private void CreateDynamicTable(DataTable dt, IEnumerable<string> groupColumns)
        {
            var normalizedGroupColumns = NormalizeGroupColumns(dt, groupColumns);
            var groupSet = normalizedGroupColumns.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var detailColumns = dt.Columns.Cast<DataColumn>()
                .Where(x => !groupSet.Contains(x.ColumnName))
                .ToList();

            var printableWidth = PageWidth - Margins.Left - Margins.Right;
            var columnCount = Math.Max(1, detailColumns.Count);
            var cellWidth = printableWidth / columnCount;

            // ================= HEADER =================
            var headerTable = new XRTable
            {
                BoundsF = new RectangleF(0, 0, printableWidth, 32f)
            };

            var headerRow = new XRTableRow();

            foreach (var col in detailColumns)
            {
                headerRow.Cells.Add(new XRTableCell
                {
                    Text = col.ColumnName,
                    BackColor = PRIMARY,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    WidthF = cellWidth,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter,
                    Padding = new DevExpress.XtraPrinting.PaddingInfo(6, 6, 0, 0),
                    Borders = DevExpress.XtraPrinting.BorderSide.All
                });
            }

            headerTable.Rows.Add(headerRow);

            var pageHeader = Bands.GetBandByType(typeof(PageHeaderBand)) as PageHeaderBand
                             ?? new PageHeaderBand();

            if (!Bands.Contains(pageHeader))
                Bands.Add(pageHeader);

            pageHeader.Controls.Clear();
            pageHeader.HeightF = 35f;
            pageHeader.Controls.Add(headerTable);

            // ================= DETAIL =================
            var detailTable = new XRTable
            {
                BoundsF = new RectangleF(0, 0, printableWidth, 25f)
            };

            var detailRow = new XRTableRow();

            foreach (var col in detailColumns)
            {
                var cell = new XRTableCell
                {
                    WidthF = cellWidth,
                    Padding = new DevExpress.XtraPrinting.PaddingInfo(6, 6, 0, 0),
                    ForeColor = DARK,
                    Borders = DevExpress.XtraPrinting.BorderSide.All,
                    TextAlignment =
                        col.DataType == typeof(int) ||
                        col.DataType == typeof(decimal) ||
                        col.DataType == typeof(double)
                            ? DevExpress.XtraPrinting.TextAlignment.MiddleRight
                            : DevExpress.XtraPrinting.TextAlignment.MiddleLeft
                };

                cell.ExpressionBindings.Add(
                    new ExpressionBinding("BeforePrint", "Text", $"[{col.ColumnName}]"));

                detailRow.Cells.Add(cell);
            }

            if (detailColumns.Count == 0)
            {
                detailRow.Cells.Add(new XRTableCell
                {
                    WidthF = printableWidth
                });
            }

            detailTable.Rows.Add(detailRow);

            // Zebra striping
            detailTable.EvenStyleName = "EvenStyle";
            detailTable.OddStyleName = "OddStyle";

            Detail.Controls.Clear();
            Detail.HeightF = 25f;
            Detail.Controls.Add(detailTable);

            // ================= STYLE =================
            var evenStyle = new XRControlStyle
            {
                Name = "EvenStyle",
                BackColor = LIGHT
            };

            var oddStyle = new XRControlStyle
            {
                Name = "OddStyle",
                BackColor = Color.White
            };

            StyleSheet.AddRange(new[] { evenStyle, oddStyle });

            // ================= GROUP =================
            var existingGroupBands = Bands.OfType<GroupHeaderBand>().ToList();
            foreach (var band in existingGroupBands)
            {
                Bands.Remove(band);
            }

            for (var i = 0; i < normalizedGroupColumns.Count; i++)
            {
                var groupColumn = normalizedGroupColumns[i];

                var groupHeader = new GroupHeaderBand
                {
                    HeightF = 30f,
                    Level = normalizedGroupColumns.Count - i - 1
                };

                groupHeader.GroupFields.Add(
                    new GroupField(groupColumn, XRColumnSortOrder.Ascending));

                var groupLabel = new XRLabel
                {
                    BoundsF = new RectangleF(0, 0, printableWidth, 30f),
                    BackColor = SECONDARY,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    Padding = new DevExpress.XtraPrinting.PaddingInfo(12 + i * 10, 0, 0, 0),
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft
                };

                groupLabel.ExpressionBindings.Add(
                    new ExpressionBinding("BeforePrint", "Text",
                        $"'{groupColumn}: ' + ToStr([{groupColumn}])"));

                groupHeader.Controls.Add(groupLabel);
                Bands.Add(groupHeader);
            }
        }
        private void ApplyHeader(string title, string parameter, List<string> groupBy)
        {
            _ = groupBy;

            // ================= REPORT HEADER =================
            var reportHeader = Bands.GetBandByType(typeof(ReportHeaderBand)) as ReportHeaderBand;
            if (reportHeader == null)
            {
                reportHeader = new ReportHeaderBand();
                Bands.Add(reportHeader);
            }

            reportHeader.Controls.Clear();
            reportHeader.HeightF = 50f;

            // ================= TITLE =================
            var printableWidth = PageWidth - Margins.Left - Margins.Right;
            var titleLabel = new XRLabel
            {

                BoundsF = new RectangleF(0, 0, printableWidth, 40f),
                Text = title ?? string.Empty,
                Font = new Font("Arial", 16, FontStyle.Bold),
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter,

                Multiline = true,
                WordWrap = true,
                CanGrow = true
            };

            reportHeader.HeightF = 80f;

            reportHeader.Controls.Add(titleLabel);

            // ================= PARAMETER (FILTER) =================
            // if (!string.IsNullOrWhiteSpace(parameter))
            // {
            //     var paramLabel = new XRLabel
            //     {
            //         BoundsF = new RectangleF(0, 30f, PageWidth - Margins.Left - Margins.Right, 20f),
            //         Font = new Font("Arial", 10, FontStyle.Italic),
            //         TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter,
            //         Text = $"Filter: {parameter}"
            //     };

            //     reportHeader.Controls.Add(paramLabel);

            //     // vẫn giữ filter logic
            //     FilterString = parameter;
            // }
        }
    }
}

