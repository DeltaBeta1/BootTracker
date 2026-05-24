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
                // Launched by auto-start on boot: show the record dialog
                Application.Run(new BootDialog());
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
