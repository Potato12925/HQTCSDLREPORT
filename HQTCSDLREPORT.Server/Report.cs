using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;

namespace HQTCSDLREPORT.Server
{
    public partial class Report : DevExpress.XtraReports.UI.XtraReport
    {
        public Report(DataTable dt)
        {
            InitializeComponent();

            this.DataSource = dt;

            CreateDynamicTable(dt);
        }

        private void CreateDynamicTable(DataTable dt)
        {
            // HEADER
            var headerBand = new PageHeaderBand();
            this.Bands.Add(headerBand);

            XRTable headerTable = new XRTable();
            XRTableRow headerRow = new XRTableRow();

            foreach (DataColumn col in dt.Columns)
            {
                XRTableCell cell = new XRTableCell
                {
                    Text = col.ColumnName,
                    BackColor = Color.LightGray,
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    WidthF = 150
                };

                headerRow.Cells.Add(cell);
            }

            headerTable.Rows.Add(headerRow);
            headerBand.Controls.Add(headerTable);

            // DETAIL
            var detailBand = new DetailBand();
            this.Bands.Add(detailBand);

            XRTable detailTable = new XRTable();
            XRTableRow detailRow = new XRTableRow();

            foreach (DataColumn col in dt.Columns)
            {
                XRTableCell cell = new XRTableCell
                {
                    WidthF = 150
                };

                cell.ExpressionBindings.Add(
                    new ExpressionBinding("BeforePrint", "Text", $"[{col.ColumnName}]")
                );

                detailRow.Cells.Add(cell);
            }

            detailTable.Rows.Add(detailRow);
            detailBand.Controls.Add(detailTable);
        }
    }
}
