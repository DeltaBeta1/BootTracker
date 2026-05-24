using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using BootTracker.Helpers;
using BootTracker.Services;

namespace BootTracker.Forms
{
    public class SettingsForm : Form
    {
        private CheckBox chkAutoStart;
        private Label lblStatus;
        private AppConfig _config;

        public SettingsForm()
        {
            _config = AppConfig.Load();

            Text = "设置 - Boot Tracker";
            Size = new Size(420, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            Controls.Add(pnl);

            var title = new Label
            {
                Text = "Boot Tracker 设置",
                Font = new Font("Microsoft YaHei", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(100, 15),
            };
            pnl.Controls.Add(title);

            chkAutoStart = new CheckBox
            {
                Text = "开机自动运行 (开机时弹出记录对话框)",
                AutoSize = true,
                Location = new Point(30, 60),
                Font = new Font("Microsoft YaHei", 10),
                Checked = AutoStartService.IsEnabled(),
            };
            chkAutoStart.CheckedChanged += OnAutoStartChanged;
            pnl.Controls.Add(chkAutoStart);

            lblStatus = new Label
            {
                AutoSize = true,
                Location = new Point(50, 95),
                ForeColor = Color.Gray,
            };
            UpdateStatusLabel();
            pnl.Controls.Add(lblStatus);

            var sep = new Label
            {
                Text = "────────────────────",
                AutoSize = true,
                ForeColor = Color.LightGray,
                Location = new Point(30, 130),
            };
            pnl.Controls.Add(sep);

            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            var lblInfo = new Label
            {
                Text = "数据存储: " + Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BootTracker\n" +
                       "版本: " + (ver != null ? ver.ToString() : "1.0"),
                AutoSize = true,
                Location = new Point(30, 160),
                ForeColor = Color.Gray,
                MaximumSize = new Size(360, 0),
            };
            pnl.Controls.Add(lblInfo);

            var btnClose = new Button
            {
                Text = "关闭",
                Size = new Size(80, 30),
                Location = new Point(160, 210),
            };
            btnClose.Click += (s, e) =>
            {
                _config.FirstRun = false;
                _config.Save();
                Close();
            };
            pnl.Controls.Add(btnClose);
        }

        private void OnAutoStartChanged(object sender, EventArgs e)
        {
            if (chkAutoStart.Checked)
            {
                string exePath = Assembly.GetExecutingAssembly().Location;
                AutoStartService.Enable(exePath);
                UpdateStatusLabel();
            }
            else
            {
                var pwd = PasswordDialog.Show("取消开机自动运行需要管理员密码", "验证管理员");
                if (pwd == "admin")
                {
                    AutoStartService.Disable();
                    UpdateStatusLabel();
                    MessageBox.Show("已取消开机自动运行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (pwd != "")
                {
                    MessageBox.Show("密码错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    chkAutoStart.CheckedChanged -= OnAutoStartChanged;
                    chkAutoStart.Checked = true;
                    chkAutoStart.CheckedChanged += OnAutoStartChanged;
                }
                else
                {
                    chkAutoStart.CheckedChanged -= OnAutoStartChanged;
                    chkAutoStart.Checked = true;
                    chkAutoStart.CheckedChanged += OnAutoStartChanged;
                }
            }
        }

        private void UpdateStatusLabel()
        {
            lblStatus.Text = chkAutoStart.Checked
                ? "状态: 已开启 - 下次开机将弹出记录对话框"
                : "状态: 未开启 - 开机不会弹窗";
        }
    }
}
