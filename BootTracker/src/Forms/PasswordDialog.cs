using System.Drawing;
using System.Windows.Forms;

namespace BootTracker.Forms
{
    public class PasswordDialog : Form
    {
        private TextBox txtPwd;

        private PasswordDialog(string prompt, string title)
        {
            Text = title;
            Size = new Size(340, 160);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = false;

            Controls.Add(new Label
            {
                Text = prompt,
                Location = new Point(15, 15),
                AutoSize = true,
            });

            txtPwd = new TextBox
            {
                Location = new Point(15, 40),
                Size = new Size(295, 23),
                PasswordChar = '*',
            };
            Controls.Add(txtPwd);

            var btnOk = new Button
            {
                Text = "确定",
                Size = new Size(75, 27),
                Location = new Point(140, 75),
                DialogResult = DialogResult.OK,
            };
            Controls.Add(btnOk);

            var btnCancel = new Button
            {
                Text = "取消",
                Size = new Size(75, 27),
                Location = new Point(225, 75),
                DialogResult = DialogResult.Cancel,
            };
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        public static string Show(string prompt, string title)
        {
            using (var dlg = new PasswordDialog(prompt, title))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    return dlg.txtPwd.Text;
                return "";
            }
        }
    }
}
