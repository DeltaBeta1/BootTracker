using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BootTracker.Services;

namespace BootTracker.Forms
{
    public class EventLogCompareForm : Form
    {
        private DateTimePicker dtFrom, dtTo;
        private DataGridView grid;
        private Label lblStatus;

        public EventLogCompareForm()
        {
            Text = "系统日志对比 - Boot Tracker";
            Size = new Size(780, 560);
            StartPosition = FormStartPosition.CenterScreen;

            var bar = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8, 5, 8, 0) };
            Controls.Add(bar);

            bar.Controls.Add(new Label { Text = "从", Location = new Point(5, 8), AutoSize = true });
            dtFrom = new DateTimePicker { Location = new Point(30, 5), Width = 125, Format = DateTimePickerFormat.Short };
            dtFrom.Value = DateTime.Now.AddDays(-30);
            bar.Controls.Add(dtFrom);

            bar.Controls.Add(new Label { Text = "到", Location = new Point(162, 8), AutoSize = true });
            dtTo = new DateTimePicker { Location = new Point(185, 5), Width = 125, Format = DateTimePickerFormat.Short };
            bar.Controls.Add(dtTo);

            var btnCompare = new Button { Text = "开始对比", Location = new Point(320, 3), Size = new Size(80, 27) };
            btnCompare.Click += (s, e) => LoadCompare();
            bar.Controls.Add(btnCompare);

            var btnExport = new Button { Text = "导出Excel", Location = new Point(410, 3), Size = new Size(80, 27) };
            btnExport.Click += (s, e) => Export();
            bar.Controls.Add(btnExport);

            lblStatus = new Label { Location = new Point(500, 8), AutoSize = true, ForeColor = Color.Gray };
            bar.Controls.Add(lblStatus);

            grid = new DataGridView
            {
                Location = new Point(0, 40),
                Size = new Size(ClientSize.Width, ClientSize.Height - 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersVisible = true,
                ColumnHeadersHeight = 28,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Microsoft YaHei", 9, FontStyle.Bold),
                    BackColor = SystemColors.Control,
                },
                BackgroundColor = SystemColors.Window,
                BorderStyle = BorderStyle.FixedSingle,
            };
            grid.Columns.Add("Day", "日期");
            grid.Columns.Add("SystemCount", "系统开机事件数");
            grid.Columns.Add("RecordedCount", "已记录开机数");
            grid.Columns.Add("Match", "是否一致");
            grid.Columns["Day"].FillWeight = 25;
            grid.Columns["SystemCount"].FillWeight = 25;
            grid.Columns["RecordedCount"].FillWeight = 25;
            grid.Columns["Match"].FillWeight = 25;
            Controls.Add(grid);

            LoadCompare();
        }

        private void LoadCompare()
        {
            grid.Rows.Clear();
            lblStatus.Text = "正在查询系统日志...";
            lblStatus.ForeColor = Color.Gray;
            Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            var data = SystemBootLogService.Compare(dtFrom.Value, dtTo.Value);

            foreach (var item in data)
            {
                int idx = grid.Rows.Add(item.Day, item.SystemCount, item.RecordedCount,
                    item.Match ? "一致" : "不一致");
                if (!item.Match)
                {
                    grid.Rows[idx].DefaultCellStyle.BackColor = Color.FromArgb(255, 199, 206);
                }
            }

            lblStatus.Text = "共 " + data.Count + " 天";
            lblStatus.ForeColor = Color.Black;
            Cursor = Cursors.Default;
        }

        private void Export()
        {
            using (var dlg = new SaveFileDialog
            {
                Filter = "Excel文件 (*.xls)|*.xls",
                FileName = "boot_compare_" + DateTime.Now.ToString("yyyyMMdd") + ".xls"
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;

                var data = new List<CompareItem>();
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.Cells["Day"].Value != null)
                        data.Add(new CompareItem
                        {
                            Day = row.Cells["Day"].Value.ToString(),
                            SystemCount = Convert.ToInt32(row.Cells["SystemCount"].Value),
                            RecordedCount = Convert.ToInt32(row.Cells["RecordedCount"].Value),
                        });
                }
                ExcelExportService.ExportCompare(data, dlg.FileName);
                MessageBox.Show("已导出到:\n" + dlg.FileName, "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
