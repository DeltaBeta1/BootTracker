using System;
using System.Windows.Forms;
using BootTracker.Forms;
using BootTracker.Helpers;
using BootTracker.Services;
using Microsoft.Win32;

namespace BootTracker
{
    public class AppTrayContext : ApplicationContext
    {
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _menu;

        public AppTrayContext()
        {
            _menu = new ContextMenuStrip();

            _menu.Items.Add("设置", null, OnSettings);
            _menu.Items.Add("管理记录", null, OnAdmin);
            _menu.Items.Add("数据统计", null, OnStats);
            _menu.Items.Add("系统日志对比", null, OnCompare);
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add("退出", null, OnExit);

            _trayIcon = new NotifyIcon
            {
                Text = "Boot Tracker - 开机记录管理",
                ContextMenuStrip = _menu,
                Visible = true,
            };

            // Use default app icon (or a simple embedded icon)
            try
            {
                _trayIcon.Icon = System.Drawing.SystemIcons.Application;
            }
            catch { }

            _trayIcon.DoubleClick += (s, e) => OnSettings(s, e);

            SystemEvents.SessionEnding += OnSessionEnding;
        }

        private void OnSessionEnding(object sender, SessionEndingEventArgs e)
        {
            try
            {
                DatabaseService.Instance.UpdateLatestShutdownTime(
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch { }
        }

        private void OnSettings(object sender, EventArgs e)
        {
            using (var form = new SettingsForm())
            {
                form.ShowDialog();
            }
        }

        private void OnAdmin(object sender, EventArgs e)
        {
            using (var form = new AdminForm())
            {
                if (!form.IsDisposed)
                    form.ShowDialog();
            }
        }

        private void OnStats(object sender, EventArgs e)
        {
            using (var form = new StatisticsForm())
            {
                form.ShowDialog();
            }
        }

        private void OnCompare(object sender, EventArgs e)
        {
            using (var form = new EventLogCompareForm())
            {
                form.ShowDialog();
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Exit();
        }
    }
}
