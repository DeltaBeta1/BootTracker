using System;
using System.Drawing;
using System.Windows.Forms;

namespace BootTracker.Forms
{
    public class RecordEditDialog : Form
    {
        private TextBox txtUser, txtReason, txtApprover;
        private DateTimePicker dtBoot;

        public string UserName { get; private set; }
        public string Reason { get; private set; }
        public string Approver { get; private set; }
        public string BootTime { get; private set; }

        public RecordEditDialog()
        {
            Text = "新增开机记录";
            Size = new Size(420, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;

            int y = 15;
            var lblTime = new Label { Text = "开机时间 *", Location = new Point(15, y + 5), AutoSize = true };
            Controls.Add(lblTime);
            dtBoot = new DateTimePicker
            {
                Location = new Point(85, y + 2),
                Size = new Size(300, 23),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm:ss",
            };
            Controls.Add(dtBoot);

            y += 35;
            var lblUser = new Label { Text = "使用者 *", Location = new Point(15, y + 5), AutoSize = true };
            Controls.Add(lblUser);
            txtUser = new TextBox { Location = new Point(85, y + 2), Size = new Size(300, 23) };
            Controls.Add(txtUser);

            y += 35;
            var lblReason = new Label { Text = "事由 *", Location = new Point(15, y + 5), AutoSize = true };
            Controls.Add(lblReason);
            txtReason = new TextBox { Location = new Point(85, y + 2), Size = new Size(300, 23) };
            Controls.Add(txtReason);

            y += 35;
            var lblApprover = new Label { Text = "审批人 *", Location = new Point(15, y + 5), AutoSize = true };
            Controls.Add(lblApprover);
            txtApprover = new TextBox { Location = new Point(85, y + 2), Size = new Size(300, 23) };
            Controls.Add(txtApprover);

            y += 45;
            var btnOk = new Button { Text = "提交", Size = new Size(80, 30), Location = new Point(110, y) };
            btnOk.Click += (s, e) => Submit();
            Controls.Add(btnOk);

            var btnCancel = new Button { Text = "取消", Size = new Size(80, 30), Location = new Point(200, y), DialogResult = DialogResult.Cancel };
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private void Submit()
        {
            UserName = txtUser.Text.Trim();
            Reason = txtReason.Text.Trim();
            Approver = txtApprover.Text.Trim();
            BootTime = dtBoot.Value.ToString("yyyy-MM-dd HH:mm:ss");

            if (string.IsNullOrEmpty(UserName))
            { MessageBox.Show("请填写使用者", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtUser.Focus(); return; }
            if (string.IsNullOrEmpty(Reason))
            { MessageBox.Show("请填写事由", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtReason.Focus(); return; }
            if (string.IsNullOrEmpty(Approver))
            { MessageBox.Show("请填写审批人", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtApprover.Focus(); return; }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
