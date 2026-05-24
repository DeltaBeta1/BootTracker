using System;
using System.Reflection;
using System.Windows.Forms;
using BootTracker.Forms;
using BootTracker.Helpers;
using BootTracker.Services;

namespace BootTracker
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Determine startup mode
            bool isBootTriggered = args.Length > 0 && args[0] == "--boot";

            if (isBootTriggered)
            {
                // Show boot record dialog first
                using (var bootDialog = new BootDialog())
                {
                    bootDialog.ShowDialog();
                }
                // Then continue as tray to capture shutdown time
                Application.Run(new AppTrayContext());
                DatabaseService.Instance.Dispose();
                return;
            }

            // Normal launch: show tray icon
            var config = AppConfig.Load();

            if (config.FirstRun)
            {
                // First run: show settings directly so user can configure
                using (var form = new SettingsForm())
                {
                    form.ShowDialog();
                }
            }

            // Start tray application
            Application.Run(new AppTrayContext());

            // Cleanup
            DatabaseService.Instance.Dispose();
        }
    }
}
