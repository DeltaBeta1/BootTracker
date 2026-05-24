using System;
using System.Drawing;
using System.Windows.Forms;
using BootTracker.Services;

namespace BootTracker.Forms
{
    public class AdminForm : Form
    {
        private DataGridView grid;
        private DateTimePicker dtFrom, dtTo;
        private ComboBox cmbUser;

        public AdminForm()
        {
            if (!Authenticate()) { Close(); return; }

            Text = "记录管理 - Boot Tracker (管理员)";
            Size = new Size(960, 580);
            StartPosition = FormStartPosition.CenterScreen;

            // ── Filter bar (Top) ──
            var bar = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8, 5, 8, 0) };
            Controls.Add(bar);

            bar.Controls.Add(new Label { Text = "从", Location = new Point(5, 8), AutoSize = true });
            dtFrom = new DateTimePicker { Location = new Point(30, 5), Width = 125, Format = DateTimePickerFormat.Short };
            dtFrom.Value = DateTime.Now.AddDays(-30);
            bar.Controls.Add(dtFrom);

            bar.Controls.Add(new Label { Text = "到", Location = new Point(162, 8), AutoSize = true });
            dtTo = new DateTimePicker { Location = new Point(185, 5), Width = 125, Format = DateTimePickerFormat.Short };
            bar.Controls.Add(dtTo);

            bar.Controls.Add(new Label { Text = "使用者", Location = new Point(320, 8), AutoSize = true });
            cmbUser = new ComboBox { Location = new Point(370, 5), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbUser.Items.Add("");
            foreach (var u in DatabaseService.Instance.GetAllUsers()) cmbUser.Items.Add(u);
            cmbUser.SelectedIndex = 0;
            bar.Controls.Add(cmbUser);

            var btnSearch = new Button { Text = "查询", Location = new Point(500, 3), Size = new Size(60, 27) };
            btnSearch.Click += (s, e) => LoadData();
            bar.Controls.Add(btnSearch);

            var btnAdd = new Button { Text = "新增记录", Location = new Point(570, 3), Size = new Size(75, 27) };
            btnAdd.Click += (s, e) => AddRecord();
            bar.Controls.Add(btnAdd);

            var btnDel = new Button { Text = "删除选中", Location = new Point(655, 3), Size = new Size(75, 27) };
            btnDel.Click += (s, e) => DeleteSelected();
            bar.Controls.Add(btnDel);

            var btnExport = new Button { Text = "导出Excel", Location = new Point(740, 3), Size = new Size(80, 27) };
            btnExport.Click += (s, e) => Export();
            bar.Controls.Add(btnExport);

            grid = new DataGridView
            {
                Location = new Point(0, 40),
                Size = new Size(ClientSize.Width, ClientSize.Height - 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
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
            grid.Columns.Add("Id", "ID");
            grid.Columns.Add("UserName", "使用者");
            grid.Columns.Add("Reason", "事由");
            grid.Columns.Add("Approver", "审批人");
            grid.Columns.Add("BootTime", "开机时间");
            grid.Columns["Id"].FillWeight = 8;
            grid.Columns["UserName"].FillWeight = 18;
            grid.Columns["Reason"].FillWeight = 30;
            grid.Columns["Approver"].FillWeight = 18;
            grid.Columns["BootTime"].FillWeight = 26;
            Controls.Add(grid);

            LoadData();
        }

        private bool Authenticate()
        {
            var result = PasswordDialog.Show("请输入管理员密码", "管理员验证");
            if (result == "admin") return true;
            if (result != "")
                MessageBox.Show("密码错误", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        private void LoadData()
        {
            grid.Rows.Clear();
            var user = cmbUser.SelectedItem != null ? cmbUser.SelectedItem.ToString() : "";
            var records = DatabaseService.Instance.GetRecords(
                dtFrom.Value.ToString("yyyy-MM-dd"),
                dtTo.Value.ToString("yyyy-MM-dd"),
                string.IsNullOrEmpty(user) ? null : user);
            foreach (var r in records)
                grid.Rows.Add(r.Id, r.UserName, r.Reason, r.Approver, r.BootTime);
        }

        private void DeleteSelected()
        {
            if (grid.SelectedRows.Count == 0) return;
            if (MessageBox.Show("确定删除选中的记录吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (DataGridViewRow row in grid.SelectedRows)
                {
                    long id = (long)row.Cells["Id"].Value;
                    DatabaseService.Instance.DeleteRecord(id);
                }
                LoadData();
            }
        }

        private void AddRecord()
        {
            using (var dlg = new RecordEditDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    DatabaseService.Instance.AddRecord(dlg.UserName, dlg.Reason, dlg.Approver, dlg.BootTime);
                    LoadData();
                }
            }
        }

        private void Export()
        {
            using (var dlg = new SaveFileDialog
            {
                Filter = "Excel文件 (*.xls)|*.xls",
                FileName = "boot_records_" + DateTime.Now.ToString("yyyyMMdd") + ".xls"
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                var user = cmbUser.SelectedItem != null ? cmbUser.SelectedItem.ToString() : "";
                var records = DatabaseService.Instance.GetRecords(
                    dtFrom.Value.ToString("yyyy-MM-dd"),
                    dtTo.Value.ToString("yyyy-MM-dd"),
                    string.IsNullOrEmpty(user) ? null : user);
                ExcelExportService.ExportRecords(records, dlg.FileName);
                MessageBox.Show("已导出到:\n" + dlg.FileName, "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
