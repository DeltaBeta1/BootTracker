using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BootTracker.Services;

namespace BootTracker.Forms
{
    public class StatisticsForm : Form
    {
        private DateTimePicker dtFrom, dtTo;
        private Chart chart1, chart2;
        private DataGridView grid;
        private ComboBox cmbUserFilter;
        private TextBox txtReasonFilter;

        public StatisticsForm()
        {
            Text = "数据统计 - Boot Tracker";
            Size = new Size(1000, 820);
            StartPosition = FormStartPosition.CenterScreen;

            // ── Records grid (Fill) — add FIRST so it's at bottom of z-order, fills after Top controls ──
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeight = 30,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Microsoft YaHei", 9, FontStyle.Bold),
                    BackColor = SystemColors.Control,
                },
            };
            grid.Columns.Add("RowNo", "序号");
            grid.Columns.Add("UserName", "使用人");
            grid.Columns.Add("BootTime", "开机时间");
            grid.Columns.Add("ShutdownTime", "关机时间");
            grid.Columns.Add("Reason", "事由");
            grid.Columns.Add("Approver", "审批人");
            grid.Columns["RowNo"].FillWeight = 6;
            grid.Columns["UserName"].FillWeight = 14;
            grid.Columns["BootTime"].FillWeight = 20;
            grid.Columns["ShutdownTime"].FillWeight = 20;
            grid.Columns["Reason"].FillWeight = 26;
            grid.Columns["Approver"].FillWeight = 14;
            Controls.Add(grid);

            // ── Filter bar (Top) — add SECOND, appears BELOW charts ──
            var filterBar = new Panel { Dock = DockStyle.Top, Height = 35, Padding = new Padding(8, 4, 8, 0) };
            Controls.Add(filterBar);

            filterBar.Controls.Add(new Label { Text = "筛选使用者:", Location = new Point(5, 7), AutoSize = true });
            cmbUserFilter = new ComboBox { Location = new Point(85, 4), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbUserFilter.SelectedIndexChanged += (s, e) => LoadGrid();
            filterBar.Controls.Add(cmbUserFilter);

            filterBar.Controls.Add(new Label { Text = "筛选事由:", Location = new Point(215, 7), AutoSize = true });
            txtReasonFilter = new TextBox { Location = new Point(285, 4), Width = 140 };
            txtReasonFilter.TextChanged += (s, e) => LoadGrid();
            filterBar.Controls.Add(txtReasonFilter);

            var btnClearFilter = new Button { Text = "清除筛选", Location = new Point(435, 2), Size = new Size(70, 25) };
            btnClearFilter.Click += (s, e) =>
            {
                cmbUserFilter.SelectedIndex = 0;
                txtReasonFilter.Text = "";
            };
            filterBar.Controls.Add(btnClearFilter);

            // ── Chart panel (Top) — add THIRD, appears BELOW date bar ──
            var chartPanel = new Panel { Dock = DockStyle.Top, Height = 260, Padding = new Padding(10) };
            Controls.Add(chartPanel);

            chart1 = new Chart { Dock = DockStyle.Left, Width = 475 };
            ConfigureChart(chart1, "每日开机次数", "日期");
            chartPanel.Controls.Add(chart1);

            chart2 = new Chart { Dock = DockStyle.Right, Width = 475 };
            ConfigureChart(chart2, "使用者开机统计", "使用者");
            chartPanel.Controls.Add(chart2);

            // ── Top bar: date range + buttons (Top) — add LAST, appears at VERY TOP ──
            var bar = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8, 5, 8, 0) };
            Controls.Add(bar);

            bar.Controls.Add(new Label { Text = "从", Location = new Point(5, 8), AutoSize = true });
            dtFrom = new DateTimePicker { Location = new Point(30, 5), Width = 125, Format = DateTimePickerFormat.Short };
            dtFrom.Value = DateTime.Now.AddDays(-30);
            bar.Controls.Add(dtFrom);

            bar.Controls.Add(new Label { Text = "到", Location = new Point(162, 8), AutoSize = true });
            dtTo = new DateTimePicker { Location = new Point(185, 5), Width = 125, Format = DateTimePickerFormat.Short };
            bar.Controls.Add(dtTo);

            var btnRefresh = new Button { Text = "刷新图表", Location = new Point(320, 3), Size = new Size(80, 27) };
            btnRefresh.Click += (s, e) => RefreshAll();
            bar.Controls.Add(btnRefresh);

            var btnExport = new Button { Text = "导出统计Excel", Location = new Point(410, 3), Size = new Size(100, 27) };
            btnExport.Click += (s, e) => Export();
            bar.Controls.Add(btnExport);

            RefreshAll();
        }

        private void ConfigureChart(Chart chart, string title, string xLabel)
        {
            var t = new Title(title);
            t.Font = new Font("Microsoft YaHei", 11, FontStyle.Bold);
            chart.Titles.Add(t);
            var area = new ChartArea();
            area.AxisX.LabelStyle.Font = new Font("Microsoft YaHei", 8);
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.Title = xLabel;
            area.AxisY.Title = "次数";
            chart.ChartAreas.Add(area);
            var series = new Series("Series1")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(68, 114, 196)
            };
            series.IsValueShownAsLabel = true;
            series.LabelForeColor = Color.Black;
            chart.Series.Add(series);
            chart.Legends.Clear();
        }

        private void RefreshAll()
        {
            LoadCharts();
            LoadGrid();
        }

        private void LoadCharts()
        {
            var from = dtFrom.Value.ToString("yyyy-MM-dd");
            var to = dtTo.Value.ToString("yyyy-MM-dd");
            var db = DatabaseService.Instance;

            var dayData = db.GetStatsByDay(from, to);
            chart1.Series[0].Points.Clear();
            foreach (var d in dayData)
            {
                var label = d.Day.Length > 5 ? d.Day.Substring(5) : d.Day;
                chart1.Series[0].Points.AddXY(label, d.Count);
            }
            chart1.Titles[0].Text = dayData.Count == 0 ? "每日开机次数 (暂无数据)" : "每日开机次数";

            var userData = db.GetStatsByUser(from, to);
            chart2.Series[0].Points.Clear();
            var colors = new[] {
                Color.FromArgb(68, 114, 196), Color.FromArgb(237, 125, 49),
                Color.FromArgb(165, 165, 165), Color.FromArgb(255, 192, 0),
                Color.FromArgb(91, 155, 213), Color.FromArgb(112, 173, 71),
            };
            int ci = 0;
            foreach (var u in userData)
            {
                int ptIdx = chart2.Series[0].Points.AddXY(u.UserName, u.Count);
                chart2.Series[0].Points[ptIdx].Color = colors[ci % colors.Length];
                ci++;
            }
            chart2.Titles[0].Text = userData.Count == 0 ? "使用者开机统计 (暂无数据)" : "使用者开机统计";

            // Update user filter combobox (preserve selection)
            var selected = cmbUserFilter.SelectedIndex > 0 ? cmbUserFilter.SelectedItem.ToString() : null;
            cmbUserFilter.SelectedIndexChanged -= (s, e) => LoadGrid();
            cmbUserFilter.Items.Clear();
            cmbUserFilter.Items.Add("(全部)");
            foreach (var u in db.GetAllUsers())
                cmbUserFilter.Items.Add(u);
            cmbUserFilter.SelectedIndex = 0;
            if (selected != null)
            {
                for (int i = 0; i < cmbUserFilter.Items.Count; i++)
                {
                    if (cmbUserFilter.Items[i].ToString() == selected)
                    { cmbUserFilter.SelectedIndex = i; break; }
                }
            }
            cmbUserFilter.SelectedIndexChanged += (s, e) => LoadGrid();
        }

        private void LoadGrid()
        {
            grid.Rows.Clear();
            var from = dtFrom.Value.ToString("yyyy-MM-dd");
            var to = dtTo.Value.ToString("yyyy-MM-dd");

            var filterUser = cmbUserFilter.SelectedIndex > 0 ? cmbUserFilter.SelectedItem.ToString() : null;
            var filterReason = string.IsNullOrEmpty(txtReasonFilter.Text.Trim()) ? null : txtReasonFilter.Text.Trim();

            var records = DatabaseService.Instance.GetRecords(from, to, filterUser);

            int rowNo = 0;
            foreach (var r in records)
            {
                if (filterReason != null && !r.Reason.Contains(filterReason))
                    continue;

                rowNo++;
                grid.Rows.Add(rowNo, r.UserName, r.BootTime,
                    string.IsNullOrEmpty(r.ShutdownTime) ? "-" : r.ShutdownTime,
                    r.Reason, r.Approver);
            }
        }

        private void Export()
        {
            using (var dlg = new SaveFileDialog
            {
                Filter = "Excel文件 (*.xls)|*.xls",
                FileName = "boot_stats_" + DateTime.Now.ToString("yyyyMMdd") + ".xls"
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                var from = dtFrom.Value.ToString("yyyy-MM-dd");
                var to = dtTo.Value.ToString("yyyy-MM-dd");
                var db = DatabaseService.Instance;
                var dayData = db.GetStatsByDay(from, to);
                var userData = db.GetStatsByUser(from, to);

                var filterUser = cmbUserFilter.SelectedIndex > 0 ? cmbUserFilter.SelectedItem.ToString() : null;
                var records = db.GetRecords(from, to, filterUser);
                var filterReason = string.IsNullOrEmpty(txtReasonFilter.Text.Trim()) ? null : txtReasonFilter.Text.Trim();
                if (filterReason != null)
                {
                    records = records.FindAll(r => r.Reason.Contains(filterReason));
                }
                ExcelExportService.ExportStats(dayData, userData, records, dlg.FileName);
                MessageBox.Show("已导出到:\n" + dlg.FileName, "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
