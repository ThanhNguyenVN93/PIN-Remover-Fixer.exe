using System;
using System.Windows.Forms;
using frm_pin_remover.Core;
using frm_pin_remover.UI;

namespace frm_pin_remover
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var guard = new SingleInstanceGuard())
            {
                if (!guard.TryAcquire())
                {
                    SingleInstanceGuard.FocusExistingInstance();
                    return;
                }

                bool startMinimized = Array.Exists(Environment.GetCommandLineArgs(), a => a.Equals("--tray", StringComparison.OrdinalIgnoreCase));
                Application.Run(new MainForm(startMinimized));
            }
        }
    }
}
