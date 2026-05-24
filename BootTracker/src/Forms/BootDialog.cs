using System;
using System.Drawing;
using System.Windows.Forms;
using BootTracker.Services;

namespace BootTracker.Forms
{
    public class BootDialog : Form
    {
        private TextBox txtUser, txtReason, txtApprover;

        public BootDialog()
        {
            Text = "开机记录 - Boot Tracker";
            Size = new Size(420, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;
            KeyPreview = true;

            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            Controls.Add(pnl);

            var title = new Label
            {
                Text = "笔记本电脑开机记录",
                Font = new Font("Microsoft YaHei", 14, FontStyle.Bold),
                AutoSize = true,
            };
            pnl.Controls.Add(title);
            title.Location = new Point(80, 10);

            var timeLabel = new Label
            {
                Text = DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss"),
                ForeColor = Color.Gray,
                AutoSize = true,
            };
            pnl.Controls.Add(timeLabel);
            timeLabel.Location = new Point(100, 40);

            int y = 75;
            txtUser = AddField(pnl, "使用者 *", y); y += 35;
            txtReason = AddField(pnl, "事由 *", y); y += 35;
            txtApprover = AddField(pnl, "审批人 *", y);

            var btnSubmit = new Button { Text = "提交记录", Size = new Size(90, 30), Location = new Point(85, 200) };
            var btnSkip = new Button { Text = "跳过", Size = new Size(90, 30), Location = new Point(185, 200) };
            btnSubmit.Click += (s, e) => Submit();
            btnSkip.Click += (s, e) => Close();
            pnl.Controls.Add(btnSubmit);
            pnl.Controls.Add(btnSkip);

            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) Close();
                if (e.KeyCode == Keys.Enter) Submit();
            };

            txtUser.TabIndex = 0;
            txtReason.TabIndex = 1;
            txtApprover.TabIndex = 2;
            ActiveControl = txtUser;
        }

        private TextBox AddField(Panel parent, string label, int y)
        {
            var lbl = new Label { Text = label, Location = new Point(10, y + 5), Size = new Size(65, 20) };
            parent.Controls.Add(lbl);
            var tb = new TextBox { Location = new Point(80, y + 2), Size = new Size(290, 23) };
            parent.Controls.Add(tb);
            return tb;
        }

        private void Submit()
        {
            var user = txtUser.Text.Trim();
            var reason = txtReason.Text.Trim();
            var approver = txtApprover.Text.Trim();

            if (string.IsNullOrEmpty(user))
            { MessageBox.Show("请填写使用者", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtUser.Focus(); return; }
            if (string.IsNullOrEmpty(reason))
            { MessageBox.Show("请填写事由", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtReason.Focus(); return; }
            if (string.IsNullOrEmpty(approver))
            { MessageBox.Show("请填写审批人", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtApprover.Focus(); return; }

            DatabaseService.Instance.AddRecord(user, reason, approver);
            Close();
        }
    }
}
