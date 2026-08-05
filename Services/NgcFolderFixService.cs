using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using frm_pin_remover.Core;

namespace frm_pin_remover.Services
{
    internal sealed class NgcFixResult
    {
        public bool Success { get; set; }
        public bool BlockedByBitLocker { get; set; }
        public string Message { get; set; }
        public string BackupZipPath { get; set; }
    }

    internal static class NgcFolderFixService
    {
        public static NgcFixResult Apply()
        {
            var bitLocker = BitLockerDiagnostics.Run();
            if (bitLocker.ProtectionOn)
            {
                AppLogger.Log(Localization.T("svc.log.actionDeleteNgc"), Localization.T("svc.ngc.blockedDetail"), Localization.T("svc.ngc.blockedResult"));
                return new NgcFixResult
                {
                    Success = false,
                    BlockedByBitLocker = true,
                    Message = Localization.T("svc.ngc.blockedMessage")
                };
            }

            if (!Directory.Exists(AppConstants.NgcFolderPath))
            {
                return new NgcFixResult { Success = true, Message = Localization.T("svc.ngc.notExistMessage") };
            }

            string zipPath = null;

            try
            {
                zipPath = BackupToZip();

                RunProcess("takeown.exe", $"/F \"{AppConstants.NgcFolderPath}\" /R /D Y", 60000);
                RunProcess("icacls.exe", $"\"{AppConstants.NgcFolderPath}\" /grant *S-1-5-32-544:F /T /C", 60000);

                Directory.Delete(AppConstants.NgcFolderPath, recursive: true);

                AppLogger.Log(Localization.T("svc.log.actionDeleteNgc"), Localization.TF("svc.ngc.successDetail", zipPath, AppConstants.NgcFolderPath), Localization.T("common.success"));
                return new NgcFixResult
                {
                    Success = true,
                    BackupZipPath = zipPath,
                    Message = Localization.T("svc.ngc.successMessage")
                };
            }
            catch (UnauthorizedAccessException ex)
            {
                AppLogger.Log(Localization.T("svc.log.actionDeleteNgc"), ex.Message, Localization.T("svc.ngc.accessDeniedResult"));
                return new NgcFixResult
                {
                    Success = false,
                    BackupZipPath = zipPath,
                    Message = Localization.T("svc.ngc.accessDeniedMessage")
                };
            }
            catch (Exception ex)
            {
                AppLogger.Log(Localization.T("svc.log.actionDeleteNgc"), ex.Message, Localization.T("common.failed"));
                return new NgcFixResult { Success = false, BackupZipPath = zipPath, Message = ex.Message };
            }
        }

        private static string BackupToZip()
        {
            if (!Directory.Exists(AppConstants.NgcBackupDirectory))
            {
                Directory.CreateDirectory(AppConstants.NgcBackupDirectory);
            }

            string zipPath = Path.Combine(AppConstants.NgcBackupDirectory, $"Ngc-backup-{DateTime.Now:yyyyMMdd-HHmmss}.zip");
            ZipFile.CreateFromDirectory(AppConstants.NgcFolderPath, zipPath, CompressionLevel.Fastest, includeBaseDirectory: false);
            return zipPath;
        }

        private static void RunProcess(string fileName, string arguments, int timeoutMs)
        {
            var psi = new ProcessStartInfo(fileName, arguments)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                process.StandardOutput.ReadToEnd();
                process.StandardError.ReadToEnd();
                if (!process.WaitForExit(timeoutMs))
                {
                    try { process.Kill(); } catch { /* best effort */ }
                    throw new TimeoutException(Localization.TF("svc.ngc.timeoutMessage", fileName));
                }
            }
        }
    }
}
