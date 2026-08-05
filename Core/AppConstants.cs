using System;

namespace frm_pin_remover.Core
{
    internal static class AppConstants
    {
        public const string MutexName = "Global\\{6F2E7B3C-6B1D-4C6E-9C0D-6B1A7B3F1A11}-PinRemovalTool";

        public const string MainWindowTitle = "PIN Remover Fixer";

        public const string RegistryRunValueName = "PinRemovalTool";

        public const string PolicySystemKeyPath = @"SOFTWARE\Policies\Microsoft\Windows\System";
        public const string PolicySystemValueName = "AllowDomainPINLogon";

        public const string PolicyManagerAuthenticationKeyPath = @"SOFTWARE\Microsoft\PolicyManager\default\Authentication";

        public const string NgcFolderPath = @"C:\Windows\ServiceProfiles\LocalService\AppData\Local\Microsoft\Ngc";

        public static readonly string LogFilePath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PinRemovalTool", "app.log");

        public static readonly string RegistryBackupFilePath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PinRemovalTool", "registry-backup.json");

        public static readonly string NgcBackupDirectory = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PinRemovalTool", "Backups");

        public static readonly string LanguageFilePath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PinRemovalTool", "language.txt");
    }
}
