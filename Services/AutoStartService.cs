using System.Diagnostics;
using Microsoft.Win32;
using frm_pin_remover.Core;

namespace frm_pin_remover.Services
{
    internal static class AutoStartService
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static bool IsEnabled()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false))
            {
                return key?.GetValue(AppConstants.RegistryRunValueName) != null;
            }
        }

        public static void SetEnabled(bool enabled)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true))
            {
                if (enabled)
                {
                    string exePath = Process.GetCurrentProcess().MainModule.FileName;
                    key.SetValue(AppConstants.RegistryRunValueName, $"\"{exePath}\" --tray");
                }
                else
                {
                    key.DeleteValue(AppConstants.RegistryRunValueName, throwOnMissingValue: false);
                }
            }
        }
    }
}
